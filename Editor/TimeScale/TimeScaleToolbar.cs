#if UNITY_6000_3_OR_NEWER
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace LiangTools.Editor.TimeControl
{
    [InitializeOnLoad]
    public static class TimeScaleToolbar
    {
        public const string PausePath = "Liang Tools/Time Scale Pause";
        public const string SliderPath = "Liang Tools/Time Scale";
        public const string ResetPath = "Liang Tools/Time Scale Reset";

        static TimeScaleToolbar()
        {
            TimeScaleService.Changed += Refresh;
        }

        [MainToolbarElement(
            PausePath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 2,
            ussName = "LiangToolsTimeScalePause")]
        public static MainToolbarElement CreatePause()
        {
            var paused = TimeScaleService.IsPaused;
            var content = new MainToolbarContent(
                null,
                Icon(paused ? "PlayButton" : "PauseButton"),
                paused ? "Resume — restore the previous time scale" : "Pause — set Time.timeScale to 0");

            return new MainToolbarToggle(content, paused, TimeScaleService.SetPaused);
        }

        [MainToolbarElement(
            SliderPath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 3,
            ussName = "LiangToolsTimeScale")]
        public static MainToolbarElement CreateSlider()
        {
            var settings = TimeScaleSettings.instance;
            var content = new MainToolbarContent(
                $"{TimeScaleService.Value:0.00}×",
                null,
                $"Time.timeScale — {settings.minimum:0.##} to {settings.maximum:0.##} " +
                $"in steps of {settings.step:0.##}.\nAlt+T resets to 1.");

            return new MainToolbarSlider(
                content,
                TimeScaleService.Value,
                settings.minimum,
                settings.maximum,
                value => TimeScaleService.Value = value,
                false);
        }

        [MainToolbarElement(
            ResetPath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 4,
            ussName = "LiangToolsTimeScaleReset")]
        public static MainToolbarElement CreateReset()
        {
            var content = new MainToolbarContent(null, Icon("Refresh"), "Reset Time.timeScale to 1");
            return new MainToolbarButton(content, TimeScaleService.Reset)
            {
                enabled = !TimeScaleService.IsDefault
            };
        }

        private static Texture2D Icon(string name)
        {
            return EditorGUIUtility.IconContent(name).image as Texture2D;
        }

        private static void Refresh()
        {
            MainToolbar.Refresh(PausePath);
            MainToolbar.Refresh(SliderPath);
            MainToolbar.Refresh(ResetPath);
        }
    }
}
#endif
