#if !UNITY_6000_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiangTools.Editor.Toolbar
{
    [InitializeOnLoad]
    internal static class LegacyMainToolbar
    {
        private const string PlayModeZone = "ToolbarZonePlayMode";
        private const string WarnedKey = "LiangTools.Toolbar.LegacyWarned";

        private static readonly List<Action> Handlers = new List<Action>();
        private static IMGUIContainer _container;

        static LegacyMainToolbar()
        {
            ScheduleAttach();
            EditorApplication.playModeStateChanged += _ => ScheduleAttach();
        }

        public static void Register(Action onGui)
        {
            if (onGui != null && !Handlers.Contains(onGui))
            {
                Handlers.Add(onGui);
                ScheduleAttach();
            }
        }

        public static void Repaint()
        {
            _container?.MarkDirtyRepaint();
        }

        private static void ScheduleAttach()
        {
            EditorApplication.delayCall += Attach;
        }

        private static void Attach()
        {
            var zone = FindPlayModeZone();
            if (zone == null)
            {
                return;
            }

            _container?.RemoveFromHierarchy();
            _container = new IMGUIContainer(OnGui);
            zone.Add(_container);
        }

        private static void OnGui()
        {
            GUILayout.BeginHorizontal();
            for (var i = 0; i < Handlers.Count; i++)
            {
                Handlers[i]?.Invoke();
            }

            GUILayout.EndHorizontal();
        }

        private static VisualElement FindPlayModeZone()
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

            var zone = root.Q(PlayModeZone);
            if (zone == null)
            {
                WarnOnce($"'{PlayModeZone}' is missing from the toolbar");
            }

            return zone;
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
