using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace LiangTools.Editor.Scenes
{
    public static class SceneSwitcherMenu
    {
        private const string Root = "Tools/Liang Tools/Scenes/";

        [MenuItem(Root + "Switch Scene…", priority = 0)]
        [Shortcut("Liang Tools/Scenes/Switch Scene", KeyCode.O, ShortcutModifiers.Alt)]
        public static void SwitchScene()
        {
            SceneSwitcherService.BuildMenu().ShowAsContext();
        }

        [MenuItem(Root + "Back to Previous Scene", priority = 1)]
        [Shortcut("Liang Tools/Scenes/Back to Previous Scene", KeyCode.P, ShortcutModifiers.Alt)]
        public static void BackToPrevious()
        {
            var active = SceneSwitcherService.ActiveScenePath;
            if (SceneSwitcherService.OpenPrevious())
            {
                SceneSwitcherService.RecordPrevious(active);
            }
        }

        [MenuItem(Root + "Back to Previous Scene", validate = true)]
        private static bool BackToPreviousValidate()
        {
            var previous = SceneSwitcherService.PreviousScenePath;
            return !string.IsNullOrEmpty(previous) && previous != SceneSwitcherService.ActiveScenePath;
        }

        [MenuItem(Root + "Refresh Scene List", priority = 20)]
        public static void RefreshSceneList()
        {
            SceneCatalog.Invalidate();
        }

        [MenuItem(Root + "Settings…", priority = 21)]
        public static void OpenSettings()
        {
            SettingsService.OpenProjectSettings(SceneSwitcherSettingsProvider.Path);
        }
    }
}
