#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
using System;
using System.Reflection;
using UnityEngine;

namespace LiangTools.Debugging
{
    /// <summary>
    /// Keeps clicks that land on the overlay from also reaching the game.
    ///
    /// IMGUI has no raycast target: <c>OnGUI</c> draws on a separate pass from uGUI, so
    /// a tap on the panel is delivered to both. Consuming the IMGUI event is not enough
    /// either, because that only stops other IMGUI handlers. The reliable lever for uGUI
    /// is disabling the active <c>EventSystem</c>, which stops it raycasting at all.
    ///
    /// The EventSystem type is reached by reflection on purpose: it lives in the ugui
    /// package, and referencing it would turn an optional dependency into a required
    /// one for a package that otherwise needs nothing.
    /// </summary>
    internal static class InputBlocker
    {
        private static Behaviour _disabled;
        private static bool _active;

        public static bool IsBlocking => _active;

        public static void SetBlocking(bool blocking)
        {
            if (blocking == _active)
            {
                return;
            }

            if (blocking)
            {
                Block();
            }
            else
            {
                Release();
            }
        }

        private static void Block()
        {
            var eventSystem = FindEventSystem();
            _active = true;

            if (eventSystem == null || !eventSystem.enabled)
            {
                return;
            }

            eventSystem.enabled = false;
            _disabled = eventSystem;
        }

        private static void Release()
        {
            _active = false;

            // The object may have been destroyed while the overlay was open, and Unity's
            // == is what detects that.
            if (_disabled != null)
            {
                _disabled.enabled = true;
            }

            _disabled = null;
        }

        private static Behaviour FindEventSystem()
        {
            try
            {
                var type = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
                var property = type?.GetProperty("current", BindingFlags.Public | BindingFlags.Static);

                return property?.GetValue(null) as Behaviour;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
#endif
