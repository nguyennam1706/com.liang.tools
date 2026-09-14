#if !UNITY_6000_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiangTools.Editor.Toolbar
{
    /// <summary>
    /// Which part of the pre-6000.3 main toolbar a tool draws into. The names map to
    /// the elements Unity builds inside <c>UnityEditor.Toolbar</c>.
    /// </summary>
    internal enum LegacyToolbarZone
    {
        PlayMode,
        LeftAlign,
        RightAlign
    }

    [InitializeOnLoad]
    internal static class LegacyMainToolbar
    {
        private const string WarnedKey = "LiangTools.Toolbar.LegacyWarned";

        private static readonly Dictionary<LegacyToolbarZone, List<Action>> Handlers =
            new Dictionary<LegacyToolbarZone, List<Action>>();

        private static readonly Dictionary<LegacyToolbarZone, IMGUIContainer> Containers =
            new Dictionary<LegacyToolbarZone, IMGUIContainer>();

        static LegacyMainToolbar()
        {
            ScheduleAttach();
            EditorApplication.playModeStateChanged += _ => ScheduleAttach();
        }

        public static void Register(Action onGui, LegacyToolbarZone zone = LegacyToolbarZone.PlayMode)
        {
            if (onGui == null)
            {
                return;
            }

            if (!Handlers.TryGetValue(zone, out var list))
            {
                list = new List<Action>();
                Handlers[zone] = list;
            }

            if (list.Contains(onGui))
            {
                return;
            }

            list.Add(onGui);
            ScheduleAttach();
        }

        public static void Repaint()
        {
            foreach (var container in Containers.Values)
            {
                container?.MarkDirtyRepaint();
            }
        }

        private static void ScheduleAttach()
        {
            EditorApplication.delayCall += Attach;
        }

        private static void Attach()
        {
            var root = FindToolbarRoot();
            if (root == null)
            {
                return;
            }

            foreach (var pair in Handlers)
            {
                var zone = root.Q(ZoneName(pair.Key));
                if (zone == null)
                {
                    WarnOnce($"'{ZoneName(pair.Key)}' is missing from the toolbar");
                    continue;
                }

                if (Containers.TryGetValue(pair.Key, out var existing))
                {
                    existing?.RemoveFromHierarchy();
                }

                var handlers = pair.Value;
                var container = new IMGUIContainer(() => Draw(handlers));
                Containers[pair.Key] = container;
                zone.Add(container);
            }
        }

        private static void Draw(List<Action> handlers)
        {
            GUILayout.BeginHorizontal();
            for (var i = 0; i < handlers.Count; i++)
            {
                handlers[i]?.Invoke();
            }

            GUILayout.EndHorizontal();
        }

        private static string ZoneName(LegacyToolbarZone zone)
        {
            switch (zone)
            {
                case LegacyToolbarZone.LeftAlign: return "ToolbarZoneLeftAlign";
                case LegacyToolbarZone.RightAlign: return "ToolbarZoneRightAlign";
                default: return "ToolbarZonePlayMode";
            }
        }

        private static VisualElement FindToolbarRoot()
        {
            var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null)
            {
                WarnOnce("UnityEditor.Toolbar was not found");
                return null;
            }

            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars.Length == 0)
            {
                return null;
            }

            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rootField?.GetValue(toolbars[0]) is not VisualElement root)
            {
                WarnOnce("the toolbar root element could not be read");
                return null;
            }

            return root;
        }

        private static void WarnOnce(string reason)
        {
            if (SessionState.GetBool(WarnedKey, false))
            {
                return;
            }

            SessionState.SetBool(WarnedKey, true);
            Debug.LogWarning(
                $"[Liang Tools] Could not attach to the main toolbar because {reason}. " +
                "The tools stay reachable from the Tools menu and their shortcuts.");
        }
    }
}
#endif
