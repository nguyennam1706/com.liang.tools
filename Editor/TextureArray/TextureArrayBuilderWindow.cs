using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LiangTools.Editor.TextureArray
{
    /// <summary>
    /// Builds a <see cref="Texture2DArray"/> asset from an ordered list of textures.
    /// The array is written next to the first texture, using GPU copies, so the sources
    /// do not have to be readable.
    /// </summary>
    public class TextureArrayBuilderWindow : EditorWindow
    {
        private readonly List<Texture2D> _textures = new List<Texture2D>();

        private ReorderableList _list;
        private Vector2 _scroll;
        private string _assetName = "TextureArray";

        [MenuItem("Tools/Liang Tools/Texture Array Builder", priority = 200)]
        private static void Open()
        {
            GetWindow<TextureArrayBuilderWindow>("Texture Array Builder");
        }

        private void OnEnable()
        {
            _list = new ReorderableList(_textures, typeof(Texture2D), true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Textures"),
                onAddCallback = _ => _textures.Add(null)
            };

            _list.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;

                _textures[index] = (Texture2D)EditorGUI.ObjectField(
                    rect, $"Layer {index}", _textures[index], typeof(Texture2D), false);
            };
        }

        private void OnGUI()
        {
            _assetName = EditorGUILayout.TextField("Asset Name", _assetName);

            var targetFolder = GetTargetFolder();
            EditorGUILayout.LabelField(
                "Save Folder", string.IsNullOrEmpty(targetFolder) ? "—" : targetFolder);

            EditorGUILayout.Space(8f);
            DrawDropArea();
            EditorGUILayout.Space(8f);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _list.DoLayoutList();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8f);

            using (new EditorGUI.DisabledScope(_textures.Count == 0))
            {
                if (GUILayout.Button("Clear", GUILayout.Height(24f)))
                {
                    _textures.Clear();
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.Space(4f);

            using (new EditorGUI.DisabledScope(!CanBuild()))
            {
                if (GUILayout.Button("Create Texture2DArray Asset", GUILayout.Height(32f)))
                {
                    CreateTextureArray();
                }
            }
        }

        private void DrawDropArea()
        {
            var dropArea = GUILayoutUtility.GetRect(0f, 60f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag Textures Here");

            var evt = Event.current;
            if (!dropArea.Contains(evt.mousePosition))
            {
                return;
            }

            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is Texture2D texture && !_textures.Contains(texture))
                    {
                        _textures.Add(texture);
                    }
                }

                evt.Use();
            }
        }

        private bool CanBuild()
        {
            if (_textures.Count == 0 || string.IsNullOrWhiteSpace(_assetName))
            {
                return false;
            }

            foreach (var texture in _textures)
            {
                if (texture == null)
                {
                    return false;
                }
            }

            return !string.IsNullOrEmpty(GetTargetFolder());
        }

        /// <summary>Folder of the first assigned texture, empty when none resolves.</summary>
        private string GetTargetFolder()
        {
            foreach (var texture in _textures)
            {
                if (texture == null)
                {
                    continue;
                }

                var assetPath = AssetDatabase.GetAssetPath(texture);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                return Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? string.Empty;
            }

            return string.Empty;
        }

        private void CreateTextureArray()
        {
            if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
            {
                EditorUtility.DisplayDialog(
                    "Unsupported",
                    "This platform cannot copy textures on the GPU, which is how the array is filled.",
                    "OK");
                return;
            }

            var first = _textures[0];
            if (!Validate(first))
            {
                return;
            }

            var folder = GetTargetFolder();
            if (string.IsNullOrEmpty(folder))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Save Folder",
                    "Could not resolve the folder of the first texture.",
                    "OK");
                return;
            }

            var path = $"{folder}/{_assetName.Trim()}.asset";

            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                var overwrite = EditorUtility.DisplayDialog(
                    "Asset Already Exists", $"{path} already exists. Overwrite it?", "Overwrite", "Cancel");

                if (!overwrite)
                {
                    return;
                }
            }

            var array = new Texture2DArray(
                first.width, first.height, _textures.Count, first.format, first.mipmapCount > 1)
            {
                // CopySerialized below copies the name onto the existing asset, so set it
                // first rather than writing an empty name over it.
                name = _assetName.Trim(),
                wrapMode = first.wrapMode,
                filterMode = first.filterMode,
                anisoLevel = first.anisoLevel
            };

            for (var i = 0; i < _textures.Count; i++)
            {
                for (var mip = 0; mip < array.mipmapCount; mip++)
                {
                    Graphics.CopyTexture(_textures[i], 0, mip, array, i, mip);
                }
            }

            array.Apply(false, false);

            var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);

            if (existing != null)
            {
                // Write into the existing object so its fileID survives, otherwise every
                // reference to the previous array goes missing.
                EditorUtility.CopySerialized(array, existing);
                DestroyImmediate(array);
                array = existing;
            }
            else
            {
                AssetDatabase.CreateAsset(array, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(array);
            Selection.activeObject = array;

            Debug.Log($"[Liang Tools] Created Texture2DArray with {_textures.Count} layers at {path}");
        }

        /// <summary>
        /// Every layer must match the first in size, format and mip count. The array is
        /// allocated from the first texture, and a layer that leaves any mip level
        /// unwritten renders as magenta along with everything using the array.
        /// </summary>
        private bool Validate(Texture2D first)
        {
            for (var i = 0; i < _textures.Count; i++)
            {
                var texture = _textures[i];

                if (texture.width != first.width || texture.height != first.height)
                {
                    Reject(i, "size", $"{first.width}x{first.height}", $"{texture.width}x{texture.height}");
                    return false;
                }

                if (texture.format != first.format)
                {
                    Reject(i, "format", first.format.ToString(), texture.format.ToString());
                    return false;
                }

                if (texture.mipmapCount != first.mipmapCount)
                {
                    Reject(i, "mip count", first.mipmapCount.ToString(), texture.mipmapCount.ToString());
                    return false;
                }
            }

            return true;
        }

        private static void Reject(int layer, string what, string expected, string actual)
        {
            EditorUtility.DisplayDialog(
                $"Mismatched {what}",
                $"Layer {layer} does not match layer 0.\n\nExpected: {expected}\nActual: {actual}",
                "OK");
        }
    }
}
