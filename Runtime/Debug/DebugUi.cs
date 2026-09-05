using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiangTools.Debugging
{
    public sealed class DebugUi
    {
        private readonly HashSet<string> _collapsed = new HashSet<string>();
        private readonly DebugSkin _skin;

        private string _openSection;

        internal DebugUi(DebugSkin skin)
        {
            _skin = skin;
        }

        public bool Section(string title)
        {
            EndSection();

            var expanded = !_collapsed.Contains(title);
            var arrow = expanded ? "▼" : "▶";

            if (GUILayout.Button($"{arrow}  {title}", _skin.Section))
            {
                if (expanded)
                {
                    _collapsed.Add(title);
                }
                else
                {
                    _collapsed.Remove(title);
                }

                expanded = !expanded;
            }

            if (expanded)
            {
                _openSection = title;
                GUILayout.BeginVertical(_skin.SectionBody);
            }

            return expanded;
        }

        public void EndSection()
        {
            if (_openSection == null)
            {
                return;
            }

            _openSection = null;
            GUILayout.EndVertical();
        }

        public void Label(string text)
        {
            GUILayout.Label(text, _skin.Label);
        }

        public void Row(string key, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(key, _skin.Key);
            GUILayout.Label(value ?? "—", _skin.Value);
            GUILayout.EndHorizontal();
        }

        public void CopyRow(string key, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(key, _skin.Key);
            GUILayout.Label(value ?? "—", _skin.Value);

            using (new GuiEnabledScope(!string.IsNullOrEmpty(value)))
            {
                if (GUILayout.Button("copy", _skin.SmallButton, GUILayout.Width(_skin.Scaled(52f))))
                {
                    GUIUtility.systemCopyBuffer = value;
#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
                    DebugOverlay.Instance?.Toast($"Copied {key}");
#endif
                }
            }

            GUILayout.EndHorizontal();
        }

        public bool Button(string label)
        {
            return GUILayout.Button(label, _skin.Button);
        }

        public bool Toggle(string label, bool value)
        {
            return GUILayout.Toggle(value, $"  {label}", _skin.Toggle);
        }

        public float Slider(string label, float value, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _skin.Key);
            var result = GUILayout.HorizontalSlider(value, min, max, _skin.Slider, _skin.SliderThumb);
            GUILayout.Label(result.ToString("0.##"), _skin.Value, GUILayout.Width(_skin.Scaled(56f)));
            GUILayout.EndHorizontal();
            return result;
        }

        public void Separator()
        {
            GUILayout.Space(_skin.Scaled(6f));
        }

        private readonly struct GuiEnabledScope : IDisposable
        {
            private readonly bool _previous;

            public GuiEnabledScope(bool enabled)
            {
                _previous = GUI.enabled;
                GUI.enabled = enabled && _previous;
            }

            public void Dispose()
            {
                GUI.enabled = _previous;
            }
        }
    }
}
