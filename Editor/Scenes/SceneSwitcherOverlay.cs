using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiangTools.Editor.Scenes
{
    [Overlay(typeof(SceneView), OverlayId, "Scene Switcher", true)]
    public sealed class SceneSwitcherOverlay : Overlay
    {
        public const string OverlayId = "liangtools-scene-switcher";

        private Button _button;

        public override VisualElement CreatePanelContent()
        {
            _button = new Button(ShowMenu) { text = Label };
            _button.tooltip = "Switch the open scene. Hold Alt to load additively.";
            _button.style.minWidth = 140f;
            _button.style.marginLeft = 0f;
            _button.style.marginRight = 0f;

            SceneSwitcherService.ActiveSceneChanged += Refresh;
            SceneCatalog.Changed += Refresh;

            var root = new VisualElement();
            root.Add(_button);
            return root;
        }

        public override void OnWillBeDestroyed()
        {
            SceneSwitcherService.ActiveSceneChanged -= Refresh;
            SceneCatalog.Changed -= Refresh;
            base.OnWillBeDestroyed();
        }

        private static string Label => $"▾  {SceneSwitcherService.ActiveSceneName}";

        private void Refresh()
        {
            if (_button != null)
            {
                _button.text = Label;
            }
        }

        private void ShowMenu()
        {
            if (Event.current != null && Event.current.alt)
            {
                ShowAdditiveMenu();
                return;
            }

            SceneSwitcherService.BuildMenu().DropDown(_button.worldBound);
        }

        private void ShowAdditiveMenu()
        {
            var menu = new GenericMenu();
            foreach (var entry in SceneCatalog.Entries)
            {
                var path = entry.Path;
                menu.AddItem(new GUIContent($"Add: {entry.Name}"), false,
                    () => SceneSwitcherService.Open(path, UnityEditor.SceneManagement.OpenSceneMode.Additive));
            }

            menu.DropDown(_button.worldBound);
        }
    }
}
