using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LiangTools.Editor.Debugging
{
    public static class DebugOverlaySettingsProvider
    {
        public const string Path = "Project/Liang Tools/Debug Overlay";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(Path, SettingsScope.Project)
            {
                label = "Debug Overlay",
                guiHandler = _ => DrawGui(),
                keywords = new HashSet<string> { "debug", "overlay", "fps", "define", "liang", "tools" }
            };
        }

        // How the overlay is opened. These are scripting defines rather than PlayerPrefs
        // because the choice has to travel into a build on a device, where the editor's
        // PlayerPrefs do not exist.
        private static void DrawOpeningSection()
        {
            EditorGUILayout.LabelField("Opening the overlay", EditorStyles.boldLabel);

            var button = DebugDefines.IsEnabledEverywhere(DebugDefines.ButtonSymbol);
            var noGesture = DebugDefines.IsEnabledEverywhere(DebugDefines.NoGestureSymbol);

            var newButton = EditorGUILayout.Toggle(
                new GUIContent(
                    "Always Show Open Button",
                    "Keeps a button on screen that opens the overlay in one press, instead of " +
                    "needing the corner tap sequence."),
                button);

            if (newButton != button)
            {
                DebugDefines.SetEnabled(newButton, DebugDefines.ButtonSymbol);
                button = newButton;
            }

            var newNoGesture = EditorGUILayout.Toggle(
                new GUIContent(
                    "Disable Tap Sequence",
                    "Turns off the top-corner tap sequence entirely."),
                noGesture);

            if (newNoGesture != noGesture)
            {
                DebugDefines.SetEnabled(newNoGesture, DebugDefines.NoGestureSymbol);
                noGesture = newNoGesture;
            }

            if (noGesture && !button)
            {
                EditorGUILayout.HelpBox(
                    "With the tap sequence off and no button, the overlay can only be opened from " +
                    "code with LiangDebug.Toggle(), or with Alt+D in the editor.",
                    MessageType.Warning);
            }
            else if (button)
            {
                EditorGUILayout.HelpBox(
                    "A button sits in the top-right corner of the screen in every build that has " +
                    "the overlay compiled in, players included. Turn it off before a public release.",
                    MessageType.Info);
            }
        }

        private static void DrawGui()
        {
            EditorGUILayout.LabelField("Scripting Define", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"The overlay always compiles in the editor and in development builds. " +
                $"{DebugDefines.Symbol} additionally puts it in release builds.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space();

            var everywhere = DebugDefines.IsEnabledEverywhere();
            var anywhere = DebugDefines.IsEnabledAnywhere();

            EditorGUI.showMixedValue = anywhere && !everywhere;
            var toggled = EditorGUILayout.Toggle(
                new GUIContent($"Define {DebugDefines.Symbol}", "Applies to every build target listed below."),
                everywhere);
            EditorGUI.showMixedValue = false;

            if (toggled != everywhere)
            {
                DebugDefines.SetEnabled(toggled);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Per build target", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var target in DebugDefines.KnownTargets)
                {
                    var enabled = DebugDefines.IsEnabled(target);
                    var value = EditorGUILayout.Toggle(target.TargetName, enabled);
                    if (value != enabled)
                    {
                        DebugDefines.SetEnabled(target, value);
                    }
                }
            }

            EditorGUILayout.Space();
            DrawOpeningSection();
            EditorGUILayout.Space();

            if (everywhere)
            {
                EditorGUILayout.HelpBox(
                    "Release builds will contain the debug overlay, including builds submitted to a store. " +
                    "Turn this off before a public release if that is not what you want.",
                    MessageType.Warning);
            }
            else if (!anywhere)
            {
                EditorGUILayout.HelpBox(
                    "Release builds contain no overlay. It still works in the editor and in development builds.",
                    MessageType.None);
            }
        }
    }
}
