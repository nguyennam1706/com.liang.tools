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

        // Keyed by zone plus which end of it, so one zone can host a group at each end
        // without the two overwriting each other's container.
        private static readonly Dictionary<(LegacyToolbarZone Zone, bool Prepend), List<Action>> Handlers =
            new Dictionary<(LegacyToolbarZone, bool), List<Action>>();

        private static readonly Dictionary<(LegacyToolbarZone Zone, bool Prepend), IMGUIContainer> Containers =
            new Dictionary<(LegacyToolbarZone, bool), IMGUIContainer>();

        static LegacyMainToolbar()
        {
            ScheduleAttach();
            EditorApplication.playModeStateChanged += _ => ScheduleAttach();
        }

        /// <param name="prepend">
        /// Insert at the start of the zone instead of appending. Adding to
        /// <c>ToolbarZonePlayMode</c> normally lands to the right of the Play, Pause and
        /// Step buttons; prepending puts the tool to their left.
        /// </param>
        public static void Register(
            Action onGui,
            LegacyToolbarZone zone = LegacyToolbarZone.PlayMode,
            bool prepend = false)
        {
            if (onGui == null)
            {
                return;
            }

            var key = (zone, prepend);

            if (!Handlers.TryGetValue(key, out var list))
            {
                list = new List<Action>();
                Handlers[key] = list;
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
                var zoneName = ZoneName(pair.Key.Zone);
                var zone = root.Q(zoneName);
                if (zone == null)
                {
                    WarnOnce($"'{zoneName}' is missing from the toolbar");
                    continue;
                }

                if (Containers.TryGetValue(pair.Key, out var existing))
                {
                    existing?.RemoveFromHierarchy();
                }

                var handlers = pair.Value;
                var container = new IMGUIContainer(() => Draw(handlers));
                Containers[pair.Key] = container;

                if (pair.Key.Prepend)
                {
                    zone.Insert(0, container);
                }
                else
                {
                    zone.Add(container);
                }
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
