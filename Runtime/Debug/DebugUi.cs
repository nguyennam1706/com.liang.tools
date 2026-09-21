using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiangTools.Debugging
{
    public enum DebugTone
    {
        Normal,
        Good,
        Warn,
        Bad
    }

    public sealed class DebugUi
    {
        private readonly HashSet<string> _collapsed = new HashSet<string>();
        private readonly DebugSkin _skin;

        private string _openSection;
        private string _pendingConfirm;
        private int _rowIndex;
        private float _contentWidth;

        /// <summary>
        /// Set by the overlay each frame: the usable width inside the scroll view.
        /// Every control sizes against it, because IMGUI will happily lay a label out
        /// past the viewport and the horizontal scrollbar is hidden.
        /// </summary>
        internal float ContentWidth
        {
            get => _contentWidth;
            set => _contentWidth = value;
        }

        /// <summary>Set while a drag-scroll is in progress, so a swipe does not select a row.</summary>
        internal bool SuppressClicks { get; set; }

        private float KeyWidth => _contentWidth > 0f ? _contentWidth * 0.40f : _skin.Scaled(150f);

        private float ValueWidth => _contentWidth > 0f ? _contentWidth * 0.52f : _skin.Scaled(190f);

        internal DebugUi(DebugSkin skin)
        {
            _skin = skin;
        }

        public bool Section(string title)
        {
            EndSection();

            var expanded = !_collapsed.Contains(title);
            var arrow = expanded ? "▾" : "▸";

            if (GUILayout.Button($"{arrow}   {title}", _skin.Section))
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
                _rowIndex = 0;
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
            Row(key, value, DebugTone.Normal);
        }

        public void Row(string key, string value, DebugTone tone)
        {
            GUILayout.BeginHorizontal(NextRowStyle());
            GUILayout.Label(key, _skin.Key, GUILayout.Width(KeyWidth));
            GUILayout.FlexibleSpace();

            using (new GuiColorScope(DebugSkin.ToneColor(tone)))
            {
                GUILayout.Label(value ?? "—", _skin.Value, GUILayout.Width(ValueWidth));
            }

            GUILayout.EndHorizontal();
        }

        public void CopyRow(string key, string value)
        {
            var buttonWidth = _skin.Scaled(48f);

            GUILayout.BeginHorizontal(NextRowStyle());
            GUILayout.Label(key, _skin.Key, GUILayout.Width(KeyWidth));
            GUILayout.FlexibleSpace();
            GUILayout.Label(value ?? "—", _skin.Value,
                GUILayout.Width(Mathf.Max(_skin.Scaled(40f), ValueWidth - buttonWidth)));

            using (new GuiEnabledScope(!string.IsNullOrEmpty(value)))
            {
                if (GUILayout.Button("copy", _skin.SmallButton, GUILayout.Width(buttonWidth)))
                {
                    GUIUtility.systemCopyBuffer = value;
                    Toast($"Copied {key}");
                }
            }

            GUILayout.EndHorizontal();
        }

        public void TextBlock(string text)
        {
            var content = string.IsNullOrEmpty(text) ? "—" : text;

            if (_contentWidth > 0f)
            {
                GUILayout.Label(content, _skin.TextBlock, GUILayout.Width(_contentWidth));
                return;
            }

            GUILayout.Label(content, _skin.TextBlock);
        }

        public void Copy(string label, string value)
        {
            using (new GuiEnabledScope(!string.IsNullOrEmpty(value)))
            {
                if (GUILayout.Button(label, _skin.Button))
                {
                    GUIUtility.systemCopyBuffer = value;
                    Toast($"Copied {value.Length} characters");
                }
            }
        }

        public bool Button(string label)
        {
            return GUILayout.Button(label, _skin.Button);
        }

        /// <summary>
        /// A button that asks before acting: the first press swaps it for the question,
        /// and only pressing that returns true.
        /// </summary>
        public bool Button(string label, string confirmQuestion)
        {
            if (string.IsNullOrEmpty(confirmQuestion))
            {
                return Button(label);
            }

            if (_pendingConfirm != label)
            {
                if (GUILayout.Button(label, _skin.Button))
                {
                    _pendingConfirm = label;
                }

                return false;
            }

            GUILayout.BeginHorizontal();
            var confirmed = GUILayout.Button(confirmQuestion, _skin.DangerButton);
            var cancelled = GUILayout.Button("Cancel", _skin.Button, GUILayout.Width(_skin.Scaled(78f)));
            GUILayout.EndHorizontal();

            if (confirmed || cancelled)
            {
                _pendingConfirm = null;
            }

            return confirmed;
        }

        public bool Toggle(string label, bool value)
        {
            GUILayout.BeginHorizontal(NextRowStyle());
            GUILayout.Label(label, _skin.Key, GUILayout.Width(KeyWidth));
            GUILayout.FlexibleSpace();

            var style = value ? _skin.PillOn : _skin.PillOff;
            if (GUILayout.Button(value ? "ON" : "OFF", style, GUILayout.Width(_skin.Scaled(46f))))
            {
                value = !value;
            }

            GUILayout.EndHorizontal();
            return value;
        }

        public float Slider(string label, float value, float min, float max)
        {
            GUILayout.BeginHorizontal(NextRowStyle());
            GUILayout.Label(label, _skin.Key, GUILayout.Width(KeyWidth));
            var result = GUILayout.HorizontalSlider(value, min, max, _skin.Slider, _skin.SliderThumb);

            using (new GuiColorScope(DebugSkin.ToneColor(DebugTone.Normal)))
            {
                GUILayout.Label(result.ToString("0.##"), _skin.Value, GUILayout.Width(_skin.Scaled(52f)));
            }

            GUILayout.EndHorizontal();
            return result;
        }

        public void Separator()
        {
            GUILayout.Space(_skin.Scaled(6f));
        }

        /// <summary>
        /// Draws a header row plus one row per entry and returns the index of the row
        /// clicked this frame, or -1. Widths apply to the leading columns; the last
        /// column takes the remaining space.
        /// </summary>
        public int Table(string[] headers, IList<string[]> rows, int selected, params float[] widths)
        {
            var clicked = -1;

            if (headers != null && headers.Length > 0)
            {
                GUILayout.BeginHorizontal(_skin.Row);
                for (var column = 0; column < headers.Length; column++)
                {
                    GUILayout.Label(headers[column], _skin.TableHeader, ColumnOption(column, headers.Length, widths));
                }

                GUILayout.EndHorizontal();
            }

            if (rows == null || rows.Count == 0)
            {
                GUILayout.Label("Nothing to show.", _skin.Key);
                return clicked;
            }

            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                if (row == null)
                {
                    continue;
                }

                var style = index == selected
                    ? _skin.SelectedRow
                    : index % 2 == 1 ? _skin.AltRow : _skin.Row;

                GUILayout.BeginHorizontal(style);
                for (var column = 0; column < row.Length; column++)
                {
                    GUILayout.Label(row[column], _skin.Cell, ColumnOption(column, row.Length, widths));
                }

                GUILayout.EndHorizontal();

                var rect = GUILayoutUtility.GetLastRect();

                // MouseUp rather than MouseDown, and never while the user is swiping:
                // on a touch screen a scroll starts with a press on a row.
                if (!SuppressClicks &&
                    Event.current.type == EventType.MouseUp &&
                    rect.Contains(Event.current.mousePosition))
                {
                    clicked = index;
                    Event.current.Use();
                }
            }

            return clicked;
        }

        // Alternating background for consecutive rows inside a section, so a long list
        // of key/value pairs stays readable. The style is applied to the row group, not
        // drawn after it, so the background lands under the text rather than over it.
        private GUIStyle NextRowStyle()
        {
            return _rowIndex++ % 2 == 1 ? _skin.AltRow : _skin.Row;
        }

        private GUILayoutOption ColumnOption(int column, int columnCount, float[] widths)
        {
            if (widths != null && column < widths.Length && column < columnCount - 1)
            {
                return GUILayout.Width(_skin.Scaled(widths[column]));
            }

            if (_contentWidth <= 0f)
            {
                return GUILayout.ExpandWidth(true);
            }

            // The last column takes what is left. Expanding instead lets a long line
            // push the row past the viewport, which is what made text run off-screen.
            var used = 0f;
            if (widths != null)
            {
                for (var i = 0; i < widths.Length && i < columnCount - 1; i++)
                {
                    used += _skin.Scaled(widths[i]);
                }
            }

            return GUILayout.Width(Mathf.Max(_skin.Scaled(60f), _contentWidth - used - _skin.Scaled(24f)));
        }

        private static void Toast(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
            DebugOverlay.Instance?.Toast(message);
#endif
        }

        private readonly struct GuiColorScope : IDisposable
        {
            private readonly Color _previous;

            public GuiColorScope(Color color)
            {
                _previous = GUI.contentColor;
                GUI.contentColor = color;
            }

            public void Dispose()
            {
                GUI.contentColor = _previous;
            }
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
