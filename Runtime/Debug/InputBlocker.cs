#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiangTools.Debugging
{
    /// <summary>
    /// Keeps clicks that land on the overlay from also reaching the game's UI.
    ///
    /// IMGUI has no raycast target: <c>OnGUI</c> draws on a separate pass from uGUI, so
    /// a tap on the panel is delivered to both. What gets switched off here is every
    /// active raycaster, which is what actually finds UI under the pointer.
    ///
    /// Note what is deliberately *not* done: disabling the <c>EventSystem</c> itself.
    /// <c>EventSystem.current</c> returns the first entry of a list the component
    /// removes itself from in <c>OnDisable</c>, so switching it off makes
    /// <c>EventSystem.current</c> null and any game code calling
    /// <c>EventSystem.current.IsPointerOverGameObject()</c> throws every frame.
    ///
    /// The types are reached by reflection on purpose: they live in the ugui package,
    /// and referencing them would turn an optional dependency into a required one for a
    /// package that otherwise needs nothing.
    /// </summary>
    internal static class InputBlocker
    {
        private static readonly List<Behaviour> Disabled = new List<Behaviour>();

        private static Type _raycasterType;
        private static bool _lookedUp;
        private static bool _active;

        public static bool IsBlocking => _active;

        public static void SetBlocking(bool blocking)
        {
            if (blocking == _active)
            {
                return;
            }

            _active = blocking;

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
            var type = ResolveRaycasterType();
            if (type == null)
            {
                return;
            }

            try
            {
                foreach (var found in UnityEngine.Object.FindObjectsByType(type, FindObjectsSortMode.None))
                {
                    if (found is Behaviour behaviour && behaviour.enabled)
                    {
                        behaviour.enabled = false;
                        Disabled.Add(behaviour);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Liang Tools] Could not block UI raycasts: {e.Message}");
            }
        }

        private static void Release()
        {
            for (var i = 0; i < Disabled.Count; i++)
            {
                // The object may have been destroyed while the overlay was open, and
                // Unity's == is what detects that.
                if (Disabled[i] != null)
                {
                    Disabled[i].enabled = true;
                }
            }

            Disabled.Clear();
        }

        // BaseRaycaster covers GraphicRaycaster and the physics raycasters alike.
        private static Type ResolveRaycasterType()
        {
            if (_lookedUp)
            {
                return _raycasterType;
            }

            _lookedUp = true;

            try
            {
                _raycasterType = Type.GetType("UnityEngine.EventSystems.BaseRaycaster, UnityEngine.UI");
            }
            catch (Exception)
            {
                _raycasterType = null;
            }

            return _raycasterType;
        }
    }
}
#endif
