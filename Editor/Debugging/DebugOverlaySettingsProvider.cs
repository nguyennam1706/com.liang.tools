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
