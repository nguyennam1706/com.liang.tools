using UnityEditor;

namespace LiangTools.Editor.Debugging
{
    [FilePath("ProjectSettings/LiangToolsDebug.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class DebugDefineSettings : ScriptableSingleton<DebugDefineSettings>
    {
        public bool installed;

        public void Persist()
        {
            Save(true);
        }
    }

    [InitializeOnLoad]
    internal static class DebugDefineInstaller
    {
        static DebugDefineInstaller()
        {
            EditorApplication.delayCall += InstallOnce;
        }

        private static void InstallOnce()
        {
            var settings = DebugDefineSettings.instance;
            if (settings.installed)
            {
                return;
            }

            settings.installed = true;
            settings.Persist();

            if (DebugDefines.IsEnabledEverywhere())
            {
                return;
            }

            DebugDefines.SetEnabled(true);
            UnityEngine.Debug.Log(
                $"[Liang Tools] Added the {DebugDefines.Symbol} scripting define so the debug overlay is present in " +
                "release builds. Turn it off in Project Settings → Liang Tools → Debug Overlay.");
        }
    }
}
