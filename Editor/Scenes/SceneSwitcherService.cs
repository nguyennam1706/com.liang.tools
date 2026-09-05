using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiangTools.Editor.Scenes
{
    [InitializeOnLoad]
    public static class SceneSwitcherService
    {
        private const string PreviousSceneKey = "LiangTools.Scenes.PreviousScenePath";

        public static event System.Action ActiveSceneChanged;

        static SceneSwitcherService()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode += (_, __) => ActiveSceneChanged?.Invoke();
        }

        public static string ActiveScenePath => SceneManager.GetActiveScene().path;

        public static string ActiveSceneName
        {
            get
            {
                var scene = SceneManager.GetActiveScene();
                return string.IsNullOrEmpty(scene.name) ? "Untitled" : scene.name;
            }
        }

        public static string PreviousScenePath
        {
            get => SessionState.GetString(PreviousSceneKey, string.Empty);
            private set => SessionState.SetString(PreviousSceneKey, value);
        }

        public static bool Open(string path, OpenSceneMode mode = OpenSceneMode.Single)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (mode == OpenSceneMode.Single)
            {
                if (path == ActiveScenePath)
                {
                    return true;
                }

                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return false;
                }
            }

            try
            {
                EditorSceneManager.OpenScene(path, mode);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Liang Tools] Could not open scene '{path}': {e.Message}");
                return false;
            }
        }

        public static bool OpenPrevious()
        {
            var previous = PreviousScenePath;
            return !string.IsNullOrEmpty(previous) && Open(previous);
        }

        public static bool OpenAtIndex(int index)
        {
            var entries = SceneCatalog.Entries;
            return index >= 0 && index < entries.Count && Open(entries[index].Path);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (mode == OpenSceneMode.Single)
            {
                ActiveSceneChanged?.Invoke();
            }
        }

        internal static void RecordPrevious(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                PreviousScenePath = path;
            }
        }

        public static GenericMenu BuildMenu()
        {
            var menu = new GenericMenu();
            var entries = SceneCatalog.Entries;
            var active = ActiveScenePath;
            var groupByFolder = SceneSwitcherSettings.instance.source != SceneSource.BuildSettings;

            if (entries.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No scenes found"));
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var label = SceneCatalog.MenuLabel(entry, groupByFolder);
                if (entry.InBuild && !entry.EnabledInBuild)
                {
                    label += " (disabled in build)";
                }

                var path = entry.Path;
                menu.AddItem(new GUIContent(label), path == active, () =>
                {
                    RecordPrevious(active);
                    Open(path);
                });
            }

            menu.AddSeparator(string.Empty);

            var previous = PreviousScenePath;
            if (string.IsNullOrEmpty(previous) || previous == active)
            {
                menu.AddDisabledItem(new GUIContent("Back to Previous Scene"));
            }
            else
            {
                menu.AddItem(new GUIContent($"Back to Previous Scene/{System.IO.Path.GetFileNameWithoutExtension(previous)}"), false, () =>
                {
                    RecordPrevious(active);
                    Open(previous);
                });
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Refresh Scene List"), false, SceneCatalog.Invalidate);
            menu.AddItem(new GUIContent("Settings…"), false,
                () => SettingsService.OpenProjectSettings(SceneSwitcherSettingsProvider.Path));

            return menu;
        }
    }
}
