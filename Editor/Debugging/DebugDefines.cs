using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace LiangTools.Editor.Debugging
{
    public static class DebugDefines
    {
        /// <summary>Compiles the overlay into non-development builds.</summary>
        public const string Symbol = "LIANG_TOOLS_DEBUG";

        /// <summary>Always shows the on-screen button that opens the overlay.</summary>
        public const string ButtonSymbol = "LIANG_TOOLS_DEBUG_BUTTON";

        /// <summary>Turns off the corner tap sequence.</summary>
        public const string NoGestureSymbol = "LIANG_TOOLS_DEBUG_NO_GESTURE";

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
            return IsEnabled(target, Symbol);
        }

        public static bool IsEnabled(NamedBuildTarget target, string symbol)
        {
            return Read(target).Contains(symbol);
        }

        public static bool IsEnabledEverywhere()
        {
            return IsEnabledEverywhere(Symbol);
        }

        public static bool IsEnabledEverywhere(string symbol)
        {
            return Targets.All(target => IsEnabled(target, symbol));
        }

        public static bool IsEnabledAnywhere()
        {
            return Targets.Any(target => IsEnabled(target, Symbol));
        }

        public static void SetEnabled(bool enabled, string symbol)
        {
            foreach (var target in Targets)
            {
                SetEnabled(target, enabled, symbol);
            }
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
            SetEnabled(target, enabled, Symbol);
        }

        public static void SetEnabled(NamedBuildTarget target, bool enabled, string symbol)
        {
            var symbols = Read(target);
            if (enabled == symbols.Contains(symbol))
            {
                return;
            }

            if (enabled)
            {
                symbols.Add(symbol);
            }
            else
            {
                symbols.Remove(symbol);
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
