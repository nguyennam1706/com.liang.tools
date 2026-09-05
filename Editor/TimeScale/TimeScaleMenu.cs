using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace LiangTools.Editor.TimeControl
{
    public static class TimeScaleMenu
    {
        private const string Root = "Tools/Liang Tools/Time Scale/";

        [MenuItem(Root + "Reset to 1", priority = 0)]
        [Shortcut("Liang Tools/Time Scale/Reset", KeyCode.T, ShortcutModifiers.Alt)]
        public static void ResetTimeScale()
        {
            TimeScaleService.Reset();
        }

        [MenuItem(Root + "Reset to 1", validate = true)]
        private static bool ResetTimeScaleValidate()
        {
            return !TimeScaleService.IsDefault;
        }

        [MenuItem(Root + "Toggle Pause", priority = 1)]
        [Shortcut("Liang Tools/Time Scale/Toggle Pause", KeyCode.Semicolon, ShortcutModifiers.Alt)]
        public static void TogglePause()
        {
            TimeScaleService.SetPaused(!TimeScaleService.IsPaused);
        }

        [MenuItem(Root + "Slower", priority = 20)]
        [Shortcut("Liang Tools/Time Scale/Slower", KeyCode.LeftBracket, ShortcutModifiers.Alt)]
        public static void Slower()
        {
            TimeScaleService.Nudge(-1);
        }

        [MenuItem(Root + "Faster", priority = 21)]
        [Shortcut("Liang Tools/Time Scale/Faster", KeyCode.RightBracket, ShortcutModifiers.Alt)]
        public static void Faster()
        {
            TimeScaleService.Nudge(1);
        }

        [MenuItem(Root + "Settings\u2026", priority = 40)]
        public static void OpenSettings()
        {
            SettingsService.OpenProjectSettings(TimeScaleSettingsProvider.Path);
        }
    }
}
