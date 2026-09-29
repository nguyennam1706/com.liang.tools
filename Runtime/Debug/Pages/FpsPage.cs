#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
using UnityEngine;

namespace LiangTools.Debugging
{
    public sealed class FpsPage : IDebugPage
    {
        public string Title => "FPS";

        public int Order => 0;

        public void Draw(DebugUi ui)
        {
            var overlay = DebugOverlay.Instance;
            if (overlay == null)
            {
                return;
            }

            var fps = overlay.Fps;

            if (ui.Section("Frame Rate"))
            {
                ui.Row("Current", $"{fps.Current:0.0} fps  ({fps.FrameTimeMs:0.00} ms)");
                ui.Row("Average", $"{fps.Average:0.0} fps");
                ui.Row("Min / Max", $"{fps.Minimum:0.0} / {fps.Maximum:0.0} fps");

                if (ui.Button("Reset statistics"))
                {
                    fps.Reset();
                }
            }

            if (ui.Section("Display"))
            {
                var show = ui.Toggle("Show FPS while the overlay is closed", overlay.ShowFpsOverlay);
                if (show != overlay.ShowFpsOverlay)
                {
                    overlay.ShowFpsOverlay = show;
                }

                var handle = ui.Toggle("Show a button to reopen this overlay", overlay.ShowHandle);
                if (handle != overlay.ShowHandle)
                {
                    overlay.ShowHandle = handle;
                }
            }

            if (ui.Section("Targets"))
            {
                ui.Row("Target frame rate", Application.targetFrameRate < 0
                    ? "platform default"
                    : Application.targetFrameRate.ToString());
                ui.Row("VSync count", QualitySettings.vSyncCount.ToString());
                ui.Row("Time scale", Time.timeScale.ToString("0.##"));

                if (ui.Button("Target 30")) Application.targetFrameRate = 30;
                if (ui.Button("Target 60")) Application.targetFrameRate = 60;
                if (ui.Button("Uncap")) Application.targetFrameRate = -1;
            }

            ui.EndSection();
        }
    }
}
#endif
