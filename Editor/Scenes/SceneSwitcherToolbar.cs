#if UNITY_6000_3_OR_NEWER
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiangTools.Editor.Scenes
{
    [InitializeOnLoad]
    public static class SceneSwitcherToolbar
    {
        public const string ElementPath = "Liang Tools/Scene Switcher";

        static SceneSwitcherToolbar()
        {
            SceneSwitcherService.ActiveSceneChanged += Refresh;
            SceneCatalog.Changed += Refresh;
        }

        [MainToolbarElement(
            ElementPath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 1,
            ussName = "LiangToolsSceneSwitcher")]
        public static MainToolbarElement Create()
        {
            var content = new MainToolbarContent(
                SceneSwitcherService.ActiveSceneName,
                EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D,
                "Switch the open scene.\nAlt+O opens this menu, Alt+P returns to the previous scene.");

            return new MainToolbarDropdown(content, OpenMenu)
            {
                populateContextMenu = PopulateContextMenu
            };
        }

        private static void Refresh()
        {
            MainToolbar.Refresh(ElementPath);
        }

        private static void OpenMenu(Rect rect)
        {
            SceneSwitcherService.BuildMenu().DropDown(rect);
        }

        private static void PopulateContextMenu(DropdownMenu menu)
        {
            menu.AppendAction("Refresh Scene List", _ => SceneCatalog.Invalidate());
            menu.AppendAction("Settings…", _ => SettingsService.OpenProjectSettings(SceneSwitcherSettingsProvider.Path));
        }
    }
}
#endif
