using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace LiangTools.Editor.Scenes
{
    public readonly struct SceneEntry
    {
        public readonly string Guid;
        public readonly string Path;
        public readonly string Name;
        public readonly bool InBuild;
        public readonly bool EnabledInBuild;

        public SceneEntry(string guid, string path, bool inBuild, bool enabledInBuild)
        {
            Guid = guid;
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            InBuild = inBuild;
            EnabledInBuild = enabledInBuild;
        }
    }

    [InitializeOnLoad]
    public static class SceneCatalog
    {
        private static List<SceneEntry> _entries;

        public static event System.Action Changed;

        static SceneCatalog()
        {
            EditorBuildSettings.sceneListChanged += Invalidate;
            SceneSwitcherSettings.instance.ApplyPlayModeStartScene();
        }

        public static IReadOnlyList<SceneEntry> Entries => _entries ??= Build();

        public static void Invalidate()
        {
            _entries = null;
            Changed?.Invoke();
        }

        private static List<SceneEntry> Build()
        {
            var settings = SceneSwitcherSettings.instance;
            var build = EditorBuildSettings.scenes;

            switch (settings.source)
            {
                case SceneSource.BuildSettings:
                    return build
                        .Where(s => !string.IsNullOrEmpty(s.path))
                        .Select(s => new SceneEntry(s.guid.ToString(), s.path, true, s.enabled))
                        .ToList();

                case SceneSource.Custom:
                    return settings.customSceneGuids
                        .Select(guid => new { guid, path = AssetDatabase.GUIDToAssetPath(guid) })
                        .Where(x => !string.IsNullOrEmpty(x.path))
                        .Select(x => MakeEntry(x.guid, x.path, build))
                        .ToList();

                default:
                    return AssetDatabase.FindAssets("t:SceneAsset")
                        .Select(guid => new { guid, path = AssetDatabase.GUIDToAssetPath(guid) })
                        .Where(x => !string.IsNullOrEmpty(x.path) && x.path.StartsWith("Assets/"))
                        .OrderBy(x => x.path)
                        .Select(x => MakeEntry(x.guid, x.path, build))
                        .ToList();
            }
        }

        private static SceneEntry MakeEntry(string guid, string path, EditorBuildSettingsScene[] build)
        {
            var inBuild = build.FirstOrDefault(s => s.path == path);
            return new SceneEntry(guid, path, inBuild != null, inBuild != null && inBuild.enabled);
        }

        public static string MenuLabel(SceneEntry entry, bool groupByFolder)
        {
            if (!groupByFolder)
            {
                return entry.Name;
            }

            var folder = Path.GetFileName(Path.GetDirectoryName(entry.Path));
            return string.IsNullOrEmpty(folder) ? entry.Name : $"{folder}/{entry.Name}";
        }
    }

    internal sealed class SceneCatalogPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (ContainsScene(imported) || ContainsScene(deleted) || ContainsScene(moved) || ContainsScene(movedFrom))
            {
                SceneCatalog.Invalidate();
            }
        }

        private static bool ContainsScene(string[] paths)
        {
            foreach (var path in paths)
            {
                if (path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
