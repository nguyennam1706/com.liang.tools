using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LiangTools.Editor.Scenes
{
    public static class SceneSwitcherSettingsProvider
    {
        public const string Path = "Project/Liang Tools/Scene Switcher";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(Path, SettingsScope.Project)
            {
                label = "Scene Switcher",
                guiHandler = _ => DrawGui(),
                keywords = new HashSet<string> { "scene", "switcher", "liang", "tools", "startup" }
            };
        }

        private static void DrawGui()
        {
            var settings = SceneSwitcherSettings.instance;

            EditorGUI.BeginChangeCheck();

            settings.source = (SceneSource)EditorGUILayout.EnumPopup(
                new GUIContent("Scene Source", "Where the switcher list comes from."), settings.source);

            if (settings.source == SceneSource.Custom)
            {
                DrawCustomList(settings);
            }

            EditorGUILayout.Space();

            settings.overridePlayModeStartScene = EditorGUILayout.Toggle(
                new GUIContent("Override Play Mode Start Scene",
                    "Always enter Play mode from a fixed scene, whatever is currently open."),
                settings.overridePlayModeStartScene);

            using (new EditorGUI.DisabledScope(!settings.overridePlayModeStartScene))
            {
                var current = LoadScene(settings.playModeStartSceneGuid);
                var picked = (SceneAsset)EditorGUILayout.ObjectField("Start Scene", current, typeof(SceneAsset), false);
                if (picked != current)
                {
                    settings.playModeStartSceneGuid = GuidOf(picked);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                settings.Persist();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                $"{SceneCatalog.Entries.Count} scene(s) listed. Alt+O opens the switcher, Alt+P jumps back to the previous scene.",
                MessageType.None);
        }

        private static void DrawCustomList(SceneSwitcherSettings settings)
        {
            EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);

            var removeAt = -1;
            for (var i = 0; i < settings.customSceneGuids.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var current = LoadScene(settings.customSceneGuids[i]);
                    var picked = (SceneAsset)EditorGUILayout.ObjectField(current, typeof(SceneAsset), false);
                    if (picked != current)
                    {
                        settings.customSceneGuids[i] = GuidOf(picked);
                    }

                    if (GUILayout.Button("−", GUILayout.Width(24f)))
                    {
                        removeAt = i;
                    }
                }
            }

            if (removeAt >= 0)
            {
                settings.customSceneGuids.RemoveAt(removeAt);
                GUI.changed = true;
            }

            if (GUILayout.Button("Add Scene", GUILayout.Width(120f)))
            {
                settings.customSceneGuids.Add(string.Empty);
                GUI.changed = true;
            }
        }

        private static SceneAsset LoadScene(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }

        private static string GuidOf(Object asset)
        {
            return asset == null ? string.Empty : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
        }
    }
}
