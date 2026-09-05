using LiangTools.Debugging;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace LiangTools.Editor.Debugging
{
    public static class DebugOverlayMenu
    {
        private const string Root = "Tools/Liang Tools/Debug Overlay/";

        [MenuItem(Root + "Toggle", priority = 0)]
        [Shortcut("Liang Tools/Debug Overlay/Toggle", KeyCode.D, ShortcutModifiers.Alt)]
        public static void Toggle()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("[Liang Tools] The debug overlay only runs in Play mode.");
                return;
            }

            LiangDebug.Toggle();
        }

        [MenuItem(Root + "Toggle", validate = true)]
        private static bool ToggleValidate()
        {
            return Application.isPlaying;
        }

        [MenuItem(Root + "Settings\u2026", priority = 20)]
        public static void OpenSettings()
        {
            SettingsService.OpenProjectSettings(DebugOverlaySettingsProvider.Path);
        }
    }
}
