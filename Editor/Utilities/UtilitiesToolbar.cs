#if UNITY_6000_3_OR_NEWER
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Toolbars;
using UnityEngine;

namespace LiangTools.Editor.Utilities
{
    [InitializeOnLoad]
    public static class UtilitiesToolbar
    {
        public const string ClearPrefsPath = "Liang Tools/Clear PlayerPrefs";
        public const string RecompilePath = "Liang Tools/Recompile";

        static UtilitiesToolbar()
        {
            // The Recompile button greys itself out while compiling or in Play mode, so
            // it has to be rebuilt whenever either changes.
            CompilationPipeline.compilationStarted += _ => RefreshRecompile();
            CompilationPipeline.compilationFinished += _ => RefreshRecompile();
            EditorApplication.playModeStateChanged += _ => RefreshRecompile();
        }

        // Play Mode Controls is the only element Unity puts in the Middle zone, at
        // index 0, so a negative index is the only way to sit to its left. The index is
        // a sort key rather than an insert position -- Unity's own Left zone has three
        // elements sharing index 11 while holding five elements in total, which
        // List.Insert could not do -- so negative values simply sort first.
        [MainToolbarElement(
            ClearPrefsPath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = -2,
            ussName = "LiangToolsClearPrefs")]
        public static MainToolbarElement CreateClearPrefs()
        {
            var content = new MainToolbarContent(
                Icon("TreeEditor.Trash") == null ? "Prefs" : null,
                Icon("TreeEditor.Trash"),
                "Clear PlayerPrefs — asks first");

            return new MainToolbarButton(content, EditorActions.ClearPlayerPrefs);
        }

        [MainToolbarElement(
            RecompilePath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = -1,
            ussName = "LiangToolsRecompile")]
        public static MainToolbarElement CreateRecompile()
        {
            var content = new MainToolbarContent(
                Icon("Refresh") == null ? "Build" : null,
                Icon("Refresh"),
                EditorActions.RecompileTooltip());

            return new MainToolbarButton(content, EditorActions.Recompile)
            {
                enabled = EditorActions.CanRecompile
            };
        }

        private static void RefreshRecompile()
        {
            MainToolbar.Refresh(RecompilePath);
        }

        private static Texture2D Icon(string name)
        {
            return EditorGUIUtility.IconContent(name).image as Texture2D;
        }
    }
}
#endif
