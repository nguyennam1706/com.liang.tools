using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace LiangTools.Editor.Scenes
{
    public enum SceneSource
    {
        BuildSettings,
        EntireProject,
        Custom
    }

    [FilePath("ProjectSettings/LiangToolsSceneSwitcher.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class SceneSwitcherSettings : ScriptableSingleton<SceneSwitcherSettings>
    {
        public SceneSource source = SceneSource.BuildSettings;
        public List<string> customSceneGuids = new List<string>();
        public bool overridePlayModeStartScene;
        public string playModeStartSceneGuid = string.Empty;

        public void Persist()
        {
            Save(true);
            SceneCatalog.Invalidate();
            ApplyPlayModeStartScene();
        }

        public void ApplyPlayModeStartScene()
        {
            if (!overridePlayModeStartScene || string.IsNullOrEmpty(playModeStartSceneGuid))
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(playModeStartSceneGuid);
            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }
    }
}
