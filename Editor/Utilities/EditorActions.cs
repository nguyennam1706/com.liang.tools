using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace LiangTools.Editor.Utilities
{
    public static class EditorActions
    {
        /// <summary>
        /// Unity does not rebuild scripts while Play mode is running or while a
        /// compilation is already under way, so the request would be silently dropped.
        /// </summary>
        public static bool CanRecompile =>
            !EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode;

        public static void ClearPlayerPrefs()
        {
            var warning = Application.isPlaying
                ? "\n\nPlay mode is running and may write some keys straight back."
                : string.Empty;

            var confirmed = EditorUtility.DisplayDialog(
                "Clear PlayerPrefs",
                $"Delete every PlayerPrefs key for this project?\n\nThis cannot be undone.{warning}",
                "Clear",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[Liang Tools] PlayerPrefs cleared.");
        }

        public static void Recompile()
        {
            if (!CanRecompile)
            {
                return;
            }

            CompilationPipeline.RequestScriptCompilation();
        }

        public static string RecompileTooltip()
        {
            if (EditorApplication.isCompiling)
            {
                return "Already compiling";
            }

            return EditorApplication.isPlayingOrWillChangePlaymode
                ? "Unavailable in Play mode"
                : "Recompile all scripts";
        }
    }
}
