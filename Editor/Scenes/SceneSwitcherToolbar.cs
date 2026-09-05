#if UNITY_6000_3_OR_NEWER
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiangTools.Editor.Scenes
{
    public static class SceneSwitcherToolbar
    {
        private static MainToolbarDropdown _dropdown;

        [MainToolbarElement(
            "Liang Tools/Scene Switcher",
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 1,
            ussName = "LiangToolsSceneSwitcher")]
        public static MainToolbarElement Create()
        {
            if (_dropdown == null)
            {
                SceneSwitcherService.ActiveSceneChanged += Refresh;
                SceneCatalog.Changed += Refresh;
            }

            _dropdown = new MainToolbarDropdown(BuildContent(), OpenMenu)
            {
                populateContextMenu = PopulateContextMenu
            };

            return _dropdown;
        }

        private static MainToolbarContent BuildContent()
        {
            return new MainToolbarContent(
                SceneSwitcherService.ActiveSceneName,
                EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D,
                "Switch the open scene.\nAlt+O opens this menu, Alt+P returns to the previous scene.");
        }

        private static void Refresh()
        {
            if (_dropdown != null)
            {
                _dropdown.content = BuildContent();
            }
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
