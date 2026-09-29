using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiangTools.Debugging;
using UnityEditor;
using UnityEngine;

namespace LiangTools.Editor.Debugging
{
    public static class DebugOverlaySettingsProvider
    {
        public const string Path = "Project/Liang Tools/Debug Overlay";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(Path, SettingsScope.Project)
            {
                label = "Debug Overlay",
                guiHandler = _ => DrawGui(),
                keywords = new HashSet<string> { "debug", "overlay", "fps", "define", "liang", "tools" }
            };
        }

        // The bottom letter row. A short list rather than the full KeyCode enum, whose
        // popup runs to ~320 entries and is unusable.
        private static readonly KeyCode[] Choices =
        {
            KeyCode.None,
            KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V,
            KeyCode.B, KeyCode.N, KeyCode.M
        };

        private static void DrawOpenKeySection()
        {
            EditorGUILayout.LabelField("Opening the overlay", EditorStyles.boldLabel);

            var settings = LiangDebugSettings.Load();
            var current = settings != null ? settings.OpenKey : LiangDebugSettings.DefaultOpenKey;

            // A key set from code, or left over from an older version, is not in the
            // list. Show it rather than silently reporting the wrong key.
            var options = Choices.Contains(current)
                ? Choices
                : new[] { current }.Concat(Choices).ToArray();

            var index = System.Array.IndexOf(options, current);

            var labels = options
                .Select(key => key == KeyCode.None ? "None (disabled)" : key.ToString())
                .ToArray();

            var picked = EditorGUILayout.Popup(
                new GUIContent("Open Key", "Pressed in Play mode and in a build to open or close the overlay."),
                index,
                labels);

            if (options[picked] != current)
            {
                Write(options[picked]);
            }

            EditorGUILayout.LabelField(
                "Stored in",
                settings != null ? AssetDatabase.GetAssetPath(settings) : $"{AssetPath} (created on change)");

            EditorGUILayout.HelpBox(
                "The key has to reach a build on a device, so it lives in a Resources asset rather " +
                "than in ProjectSettings. Code can still override it at startup with LiangDebug.OpenKey.",
                MessageType.None);
        }

        private const string AssetFolder = "Assets/Resources";
        private const string AssetPath = AssetFolder + "/" + LiangDebugSettings.ResourceName + ".asset";

        private static void Write(KeyCode key)
        {
            var settings = LiangDebugSettings.Load();

            if (settings == null)
            {
                if (!Directory.Exists(AssetFolder))
                {
                    Directory.CreateDirectory(AssetFolder);
                    AssetDatabase.Refresh();
                }

                settings = ScriptableObject.CreateInstance<LiangDebugSettings>();
                AssetDatabase.CreateAsset(settings, AssetPath);
            }

            settings.OpenKey = key;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            // The running session caches the key on first read, so push it through.
            LiangDebug.OpenKey = key;
        }

        private static void DrawGui()
        {
            EditorGUILayout.LabelField("Scripting Define", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"The overlay always compiles in the editor and in development builds. " +
                $"{DebugDefines.Symbol} additionally puts it in release builds.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space();

            var everywhere = DebugDefines.IsEnabledEverywhere();
            var anywhere = DebugDefines.IsEnabledAnywhere();

            EditorGUI.showMixedValue = anywhere && !everywhere;
            var toggled = EditorGUILayout.Toggle(
                new GUIContent($"Define {DebugDefines.Symbol}", "Applies to every build target listed below."),
                everywhere);
            EditorGUI.showMixedValue = false;

            if (toggled != everywhere)
            {
                DebugDefines.SetEnabled(toggled);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Per build target", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var target in DebugDefines.KnownTargets)
                {
                    var enabled = DebugDefines.IsEnabled(target);
                    var value = EditorGUILayout.Toggle(target.TargetName, enabled);
                    if (value != enabled)
                    {
                        DebugDefines.SetEnabled(target, value);
                    }
                }
            }

            EditorGUILayout.Space();
            DrawOpenKeySection();
            EditorGUILayout.Space();

            if (everywhere)
            {
                EditorGUILayout.HelpBox(
                    "Release builds will contain the debug overlay, including builds submitted to a store. " +
                    "Turn this off before a public release if that is not what you want.",
                    MessageType.Warning);
            }
            else if (!anywhere)
            {
                EditorGUILayout.HelpBox(
                    "Release builds contain no overlay. It still works in the editor and in development builds.",
                    MessageType.None);
            }
        }
    }
}
