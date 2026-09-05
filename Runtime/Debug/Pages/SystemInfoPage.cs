#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
using UnityEngine;

namespace LiangTools.Debugging
{
    public sealed class SystemInfoPage : IDebugPage
    {
        public string Title => "System";

        public int Order => 10;

        public void Draw(DebugUi ui)
        {
            if (ui.Section("Application"))
            {
                ui.CopyRow("Bundle ID", Application.identifier);
                ui.CopyRow("Version", Application.version);
                ui.CopyRow("Unity", Application.unityVersion);
                ui.Row("Platform", Application.platform.ToString());
                ui.Row("Debug build", Debug.isDebugBuild.ToString());
                ui.Row("Install mode", Application.installMode.ToString());
                ui.Row("System language", Application.systemLanguage.ToString());
            }

            if (ui.Section("Device"))
            {
                ui.CopyRow("Model", SystemInfo.deviceModel);
                ui.CopyRow("Device ID", SystemInfo.deviceUniqueIdentifier);
                ui.CopyRow("Operating system", SystemInfo.operatingSystem);
                ui.Row("Type", SystemInfo.deviceType.ToString());
                ui.Row("Processor", $"{SystemInfo.processorType} × {SystemInfo.processorCount}");
                ui.Row("Processor frequency", $"{SystemInfo.processorFrequency} MHz");
                ui.Row("System memory", $"{SystemInfo.systemMemorySize} MB");
                ui.Row("Battery", $"{SystemInfo.batteryLevel:P0} ({SystemInfo.batteryStatus})");
            }

            if (ui.Section("Graphics"))
            {
                ui.Row("Device", SystemInfo.graphicsDeviceName);
                ui.Row("API", SystemInfo.graphicsDeviceType.ToString());
                ui.Row("Vendor", SystemInfo.graphicsDeviceVendor);
                ui.Row("Version", SystemInfo.graphicsDeviceVersion);
                ui.Row("Memory", $"{SystemInfo.graphicsMemorySize} MB");
                ui.Row("Shader level", SystemInfo.graphicsShaderLevel.ToString());
                ui.Row("Max texture size", $"{SystemInfo.maxTextureSize} px");
            }

            if (ui.Section("Screen"))
            {
                ui.Row("Resolution", $"{Screen.width} × {Screen.height}");
                ui.Row("Native resolution", $"{Screen.currentResolution.width} × {Screen.currentResolution.height}");
                ui.Row("DPI", Screen.dpi.ToString("0.#"));
                ui.Row("Orientation", Screen.orientation.ToString());
                ui.Row("Safe area", Screen.safeArea.ToString());
                ui.Row("Refresh rate", $"{Screen.currentResolution.refreshRateRatio.value:0.##} Hz");
            }

            if (ui.Section("Memory"))
            {
                ui.Row("Mono heap", $"{System.GC.GetTotalMemory(false) / (1024f * 1024f):0.0} MB");
                ui.Row("Graphics memory", $"{SystemInfo.graphicsMemorySize} MB");

                if (ui.Button("Collect garbage"))
                {
                    System.GC.Collect();
                }

                if (ui.Button("Unload unused assets"))
                {
                    Resources.UnloadUnusedAssets();
                }
            }

            ui.EndSection();
        }
    }
}
#endif
