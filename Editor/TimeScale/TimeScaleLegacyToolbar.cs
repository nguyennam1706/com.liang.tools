#if !UNITY_6000_3_OR_NEWER
using LiangTools.Editor.Toolbar;
using UnityEditor;
using UnityEngine;

namespace LiangTools.Editor.TimeControl
{
    [InitializeOnLoad]
    internal static class TimeScaleLegacyToolbar
    {
        private static GUIStyle _valueStyle;

        static TimeScaleLegacyToolbar()
        {
            LegacyMainToolbar.Register(OnGui);
            TimeScaleService.Changed += LegacyMainToolbar.Repaint;
        }

        private static void OnGui()
        {
            _valueStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            var settings = TimeScaleSettings.instance;
            var paused = TimeScaleService.IsPaused;

            GUILayout.Space(6f);
            DrawSeparator();
            GUILayout.Space(4f);

            var toggleIcon = EditorGUIUtility.IconContent(paused ? "PlayButton" : "PauseButton").image;
            var toggleTip = paused ? "Resume" : "Pause (0×)";
            if (GUILayout.Button(new GUIContent(toggleIcon, toggleTip), EditorStyles.toolbarButton, GUILayout.Width(28f)))
            {
                TimeScaleService.SetPaused(!paused);
            }

            using (new EditorGUI.DisabledScope(TimeScaleService.IsDefault))
            {
                var resetIcon = EditorGUIUtility.IconContent("Refresh").image;
                if (GUILayout.Button(new GUIContent(resetIcon, "Reset to 1×"), EditorStyles.toolbarButton, GUILayout.Width(28f)))
                {
                    TimeScaleService.Reset();
                }
            }

            EditorGUI.BeginChangeCheck();
            var value = GUILayout.HorizontalSlider(
                TimeScaleService.Value, settings.minimum, settings.maximum,
                GUILayout.Width(110f), GUILayout.Height(18f));
            if (EditorGUI.EndChangeCheck())
            {
                TimeScaleService.Value = value;
            }

            GUILayout.Label($"{TimeScaleService.Value:0.00}×", _valueStyle, GUILayout.Width(46f));
            GUILayout.Space(6f);
        }

        private static void DrawSeparator()
        {
            var rect = GUILayoutUtility.GetRect(1f, 16f, GUILayout.Width(1f), GUILayout.ExpandHeight(false));
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.35f));
        }
    }
}
#endif
