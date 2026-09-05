#if !UNITY_6000_3_OR_NEWER
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiangTools.Editor.Scenes
{
    [InitializeOnLoad]
    internal static class SceneSwitcherLegacyToolbar
    {
        private const string PlayModeZone = "ToolbarZonePlayMode";
        private const string WarnedKey = "LiangTools.Scenes.LegacyToolbarWarned";

        private static IMGUIContainer _container;
        private static GUIStyle _style;

        static SceneSwitcherLegacyToolbar()
        {
            ScheduleAttach();
            EditorApplication.playModeStateChanged += _ => ScheduleAttach();
            SceneSwitcherService.ActiveSceneChanged += Repaint;
            SceneCatalog.Changed += Repaint;
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
                $"[Liang Tools] Scene Switcher could not attach to the main toolbar because {reason}. " +
                "Use Alt+O, the Tools menu, or enable the Scene Switcher overlay in the Scene View instead.");
        }

        private static void OnGui()
        {
            _style ??= new GUIStyle(EditorStyles.toolbarDropDown) { fixedHeight = 20f };

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                var name = SceneSwitcherService.ActiveSceneName;
                var label = name.Length > 18 ? name.Substring(0, 15) + "…" : name;
                var rect = GUILayoutUtility.GetRect(new GUIContent(label, name), _style, GUILayout.MinWidth(120f));

                if (GUI.Button(rect, new GUIContent(label, name), _style))
                {
                    SceneSwitcherService.BuildMenu().DropDown(rect);
                }
            }
        }

        private static void Repaint()
        {
            _container?.MarkDirtyRepaint();
        }
    }
}
#endif
