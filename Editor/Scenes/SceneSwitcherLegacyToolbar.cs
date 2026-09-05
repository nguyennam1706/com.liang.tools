#if !UNITY_6000_3_OR_NEWER
using LiangTools.Editor.Toolbar;
using UnityEditor;
using UnityEngine;

namespace LiangTools.Editor.Scenes
{
    [InitializeOnLoad]
    internal static class SceneSwitcherLegacyToolbar
    {
        private static GUIStyle _style;

        static SceneSwitcherLegacyToolbar()
        {
            LegacyMainToolbar.Register(OnGui);
            SceneSwitcherService.ActiveSceneChanged += LegacyMainToolbar.Repaint;
            SceneCatalog.Changed += LegacyMainToolbar.Repaint;
        }

        private static void OnGui()
        {
            _style ??= new GUIStyle(EditorStyles.toolbarDropDown) { fixedHeight = 20f };

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                var name = SceneSwitcherService.ActiveSceneName;
                var label = name.Length > 18 ? name.Substring(0, 15) + "…" : name;
                var content = new GUIContent(label, name);
                var rect = GUILayoutUtility.GetRect(content, _style, GUILayout.MinWidth(120f));

                if (GUI.Button(rect, content, _style))
                {
                    SceneSwitcherService.BuildMenu().DropDown(rect);
                }
            }
        }
    }
}
#endif
