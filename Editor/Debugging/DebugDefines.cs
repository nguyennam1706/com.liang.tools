using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace LiangTools.Editor.Debugging
{
    public static class DebugDefines
    {
        public const string Symbol = "LIANG_TOOLS_DEBUG";

        private static readonly NamedBuildTarget[] Targets =
        {
            NamedBuildTarget.Standalone,
            NamedBuildTarget.Android,
            NamedBuildTarget.iOS,
            NamedBuildTarget.WebGL,
            NamedBuildTarget.tvOS,
            NamedBuildTarget.WindowsStoreApps
        };

        public static IReadOnlyList<NamedBuildTarget> KnownTargets => Targets;

        public static bool IsEnabled(NamedBuildTarget target)
        {
            return Read(target).Contains(Symbol);
        }

        public static bool IsEnabledEverywhere()
        {
            return Targets.All(IsEnabled);
        }

        public static bool IsEnabledAnywhere()
        {
            return Targets.Any(IsEnabled);
        }

        public static void SetEnabled(bool enabled)
        {
            foreach (var target in Targets)
            {
                SetEnabled(target, enabled);
            }
        }

        public static void SetEnabled(NamedBuildTarget target, bool enabled)
        {
            var symbols = Read(target);
            if (enabled == symbols.Contains(Symbol))
            {
                return;
            }

            if (enabled)
            {
                symbols.Add(Symbol);
            }
            else
            {
                symbols.Remove(Symbol);
            }

            try
            {
                PlayerSettings.SetScriptingDefineSymbols(target, symbols.ToArray());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(
                    $"[Liang Tools] Could not write scripting defines for {target.TargetName}: {e.Message}");
            }
        }

        private static List<string> Read(NamedBuildTarget target)
        {
            try
            {
                return PlayerSettings.GetScriptingDefineSymbols(target)
                    .Split(';')
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}
