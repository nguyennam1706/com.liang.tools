using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LiangTools.Editor.TimeControl
{
    public static class TimeScaleSettingsProvider
    {
        public const string Path = "Project/Liang Tools/Time Scale";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(Path, SettingsScope.Project)
            {
                label = "Time Scale",
                guiHandler = _ => DrawGui(),
                keywords = new HashSet<string> { "time", "timescale", "slow", "speed", "liang", "tools" }
            };
        }

        private static string Stops(TimeScaleSettings settings)
        {
            if (settings.step <= 0f)
            {
                return "continuous";
            }

            var parts = new List<string>();
            for (var v = settings.minimum; v <= settings.maximum + 0.0001f && parts.Count < 24; v += settings.step)
            {
                parts.Add($"{v:0.##}");
            }

            return string.Join(", ", parts);
        }

        private static void DrawGui()
        {
            var settings = TimeScaleSettings.instance;

            EditorGUI.BeginChangeCheck();

            settings.minimum = EditorGUILayout.FloatField(
                new GUIContent("Minimum", "Left end of the toolbar slider."), settings.minimum);
            settings.maximum = EditorGUILayout.FloatField(
                new GUIContent("Maximum", "Right end of the toolbar slider."), settings.maximum);
            settings.step = EditorGUILayout.FloatField(
                new GUIContent("Step", "The slider snaps to multiples of this. Set 0 for a continuous slider."),
                settings.step);

            EditorGUILayout.Space();

            settings.reapplyOnEnteringPlayMode = EditorGUILayout.Toggle(
                new GUIContent("Reapply On Entering Play Mode",
                    "Unity resets Time.timeScale when Play mode starts; put the chosen scale back."),
                settings.reapplyOnEnteringPlayMode);

            if (EditorGUI.EndChangeCheck())
            {
                settings.Persist();
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Current", $"{TimeScaleService.Value:0.###}");
                using (new EditorGUI.DisabledScope(TimeScaleService.IsDefault))
                {
                    if (GUILayout.Button("Reset to 1", GUILayout.Width(100f)))
                    {
                        TimeScaleService.Reset();
                    }
                }
            }

            EditorGUILayout.HelpBox(
                $"Snapping to {settings.step:0.##} gives these stops: {Stops(settings)}.\n" +
                "Time.timeScale is a runtime value and only takes effect in Play mode.",
                MessageType.None);
        }
    }
}
