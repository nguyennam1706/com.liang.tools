using UnityEditor;
using UnityEditor.ShortcutManagement;

namespace LiangTools.Editor.Utilities
{
    public static class UtilitiesMenu
    {
        private const string Root = "Tools/Liang Tools/";

        [MenuItem(Root + "Clear PlayerPrefs", priority = 100)]
        [Shortcut("Liang Tools/Clear PlayerPrefs")]
        public static void ClearPlayerPrefs()
        {
            EditorActions.ClearPlayerPrefs();
        }

        [MenuItem(Root + "Recompile", priority = 101)]
        [Shortcut("Liang Tools/Recompile")]
        public static void Recompile()
        {
            EditorActions.Recompile();
        }

        [MenuItem(Root + "Recompile", validate = true)]
        private static bool RecompileValidate()
        {
            return EditorActions.CanRecompile;
        }
    }
}
