using System.Collections.Generic;
using UnityEngine;

namespace LiangTools.Debugging
{
    public static class LiangDebug
    {
        public const string EnabledSymbol = "LIANG_TOOLS_DEBUG";

        private static readonly List<IDebugPage> Pages = new List<IDebugPage>();

        public static IReadOnlyList<IDebugPage> RegisteredPages => Pages;

        public static bool IsAvailable
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static void Register(IDebugPage page)
        {
            if (page == null || Pages.Contains(page))
            {
                return;
            }

            Pages.Add(page);
            Pages.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public static void Unregister(IDebugPage page)
        {
            Pages.Remove(page);
        }

        public static void Open()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
            DebugOverlay.Instance?.SetOpen(true);
#endif
        }

        public static void Close()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
            DebugOverlay.Instance?.SetOpen(false);
#endif
        }

        public static void Toggle()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
            var overlay = DebugOverlay.Instance;
            if (overlay != null)
            {
                overlay.SetOpen(!overlay.IsOpen);
            }
#endif
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForDomainReload()
        {
            Pages.Clear();
        }
#endif
    }
}
