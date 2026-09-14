#if !UNITY_6000_3_OR_NEWER
using LiangTools.Editor.Toolbar;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace LiangTools.Editor.Utilities
{
    [InitializeOnLoad]
    internal static class UtilitiesLegacyToolbar
    {
        static UtilitiesLegacyToolbar()
        {
            LegacyMainToolbar.Register(OnGui, LegacyToolbarZone.LeftAlign);
            CompilationPipeline.compilationStarted += _ => LegacyMainToolbar.Repaint();
            CompilationPipeline.compilationFinished += _ => LegacyMainToolbar.Repaint();
            EditorApplication.playModeStateChanged += _ => LegacyMainToolbar.Repaint();
        }

        private static void OnGui()
        {
            GUILayout.Space(4f);

            var trash = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
            var clearContent = trash != null
                ? new GUIContent(trash, "Clear PlayerPrefs — asks first")
                : new GUIContent("Prefs", "Clear PlayerPrefs — asks first");

            if (GUILayout.Button(clearContent, EditorStyles.toolbarButton, GUILayout.Width(28f)))
            {
                EditorActions.ClearPlayerPrefs();
            }

            using (new EditorGUI.DisabledScope(!EditorActions.CanRecompile))
            {
                var refresh = EditorGUIUtility.IconContent("Refresh").image;
                var recompileContent = refresh != null
                    ? new GUIContent(refresh, EditorActions.RecompileTooltip())
                    : new GUIContent("Build", EditorActions.RecompileTooltip());

                if (GUILayout.Button(recompileContent, EditorStyles.toolbarButton, GUILayout.Width(28f)))
                {
                    EditorActions.Recompile();
                }
            }
        }
    }
}
#endif
