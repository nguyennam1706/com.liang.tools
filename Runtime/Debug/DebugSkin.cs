using UnityEngine;

namespace LiangTools.Debugging
{
    /// <summary>
    /// Every style the overlay draws with, built once. IMGUI has no rounded corners of
    /// its own, so the backgrounds here are generated as antialiased nine-slice
    /// textures: a texture of side 2r+1 with <see cref="GUIStyle.border"/> set to r, so
    /// the single middle pixel stretches and the corners stay crisp at any size.
    /// </summary>
    internal sealed class DebugSkin
    {
        private const float ReferenceDpi = 160f;

        private static readonly Color Ground = new Color(0.055f, 0.063f, 0.078f, 0.98f);
        private static readonly Color Panel = new Color(1f, 1f, 1f, 0.045f);
        private static readonly Color Raised = new Color(1f, 1f, 1f, 0.075f);
        private static readonly Color Hairline = new Color(1f, 1f, 1f, 0.10f);
        private static readonly Color TextPrimary = new Color(0.91f, 0.92f, 0.94f);
        private static readonly Color TextMuted = new Color(0.58f, 0.61f, 0.66f);
        private static readonly Color Accent = new Color(0.30f, 0.55f, 1f);
        private static readonly Color Danger = new Color(0.72f, 0.24f, 0.24f);

        private readonly float _scale;

        public GUIStyle Window { get; }
        public GUIStyle Scrim { get; }
        public GUIStyle Title { get; }
        public GUIStyle Subtitle { get; }
        public GUIStyle Close { get; }
        public GUIStyle Tab { get; }
        public GUIStyle ActiveTab { get; }
        public GUIStyle Section { get; }
        public GUIStyle SectionBody { get; }
        public GUIStyle Label { get; }
        public GUIStyle Key { get; }
        public GUIStyle Value { get; }
        public GUIStyle Cell { get; }
        public GUIStyle TableHeader { get; }
        public GUIStyle Row { get; }
        public GUIStyle AltRow { get; }
        public GUIStyle SelectedRow { get; }
        public GUIStyle Button { get; }
        public GUIStyle SmallButton { get; }
        public GUIStyle DangerButton { get; }
        public GUIStyle PillOn { get; }
        public GUIStyle PillOff { get; }
        public GUIStyle TextBlock { get; }
        public GUIStyle Slider { get; }
        public GUIStyle SliderThumb { get; }
        public GUIStyle Overlay { get; }
        public GUIStyle Toast { get; }
        public GUIStyle Handle { get; }

        public DebugSkin()
        {
            var dpi = Screen.dpi > 1f ? Screen.dpi : ReferenceDpi;
            _scale = Mathf.Clamp(dpi / ReferenceDpi, 1f, 3.5f);

            var body = Round(14f);
            var small = Round(12f);

            Scrim = new GUIStyle { normal = { background = Solid(new Color(0f, 0f, 0f, 0.55f)) } };

            Window = new GUIStyle
            {
                padding = Pad(14f),
                normal = { background = Rounded(10f, Ground, Hairline) },
                border = Border(10f)
            };

            Label = new GUIStyle
            {
                font = GUI.skin.label.font,
                fontSize = body,
                wordWrap = true,
                richText = true,
                normal = { textColor = TextPrimary },
                padding = new RectOffset(0, 0, Round(3f), Round(3f))
            };

            Title = new GUIStyle(Label)
            {
                fontSize = Round(17f),
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };

            Subtitle = new GUIStyle(Label)
            {
                fontSize = small,
                wordWrap = false,
                normal = { textColor = TextMuted }
            };

            // Clipped, not wrapped: a long key stays one line, and without clipping IMGUI
            // draws it straight past the width it was given.
            Key = new GUIStyle(Label)
            {
                normal = { textColor = TextMuted },
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            Value = new GUIStyle(Label) { alignment = TextAnchor.MiddleRight };
            Cell = new GUIStyle(Label)
            {
                fontSize = small,
                wordWrap = false,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft
            };

            TableHeader = new GUIStyle(Cell)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = TextMuted }
            };

            Close = new GUIStyle(Label)
            {
                fontSize = body,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                fixedHeight = Scaled(26f),
                normal = { background = Rounded(6f, Panel), textColor = TextMuted },
                hover = { background = Rounded(6f, Danger), textColor = Color.white },
                active = { background = Rounded(6f, Danger), textColor = Color.white },
                border = Border(6f)
            };

            Tab = new GUIStyle(Label)
            {
                fontSize = small,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                fixedHeight = Scaled(28f),
                padding = new RectOffset(Round(10f), Round(10f), 0, 0),
                margin = new RectOffset(0, Round(4f), 0, Round(4f)),
                normal = { background = Rounded(14f, Panel), textColor = TextMuted },
                hover = { background = Rounded(14f, Raised), textColor = TextPrimary },
                border = Border(14f)
            };

            ActiveTab = new GUIStyle(Tab)
            {
                fontStyle = FontStyle.Bold,
                normal = { background = Rounded(14f, Accent), textColor = Color.white },
                hover = { background = Rounded(14f, Accent), textColor = Color.white }
            };

            // A flat header with an accent bar down the left, not a button: sections are
            // structure, and styling them as buttons made the whole page read as a
            // stack of controls.
            Section = new GUIStyle(Label)
            {
                fontSize = body,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                fixedHeight = Scaled(30f),
                padding = new RectOffset(Round(10f), Round(8f), 0, 0),
                margin = new RectOffset(0, 0, Round(8f), 0),
                normal = { background = SectionBar(Panel), textColor = TextPrimary },
                hover = { background = SectionBar(Raised), textColor = Color.white },
                border = new RectOffset(Round(6f), Round(6f), Round(6f), Round(6f))
            };

            SectionBody = new GUIStyle
            {
                padding = new RectOffset(Round(10f), Round(8f), Round(4f), Round(8f))
            };

            Row = new GUIStyle { padding = new RectOffset(Round(6f), Round(6f), Round(2f), Round(2f)) };
            AltRow = new GUIStyle(Row) { normal = { background = Solid(new Color(1f, 1f, 1f, 0.03f)) } };
            SelectedRow = new GUIStyle(Row)
            {
                normal = { background = Rounded(4f, new Color(0.30f, 0.55f, 1f, 0.30f)) },
                border = Border(4f)
            };

            Button = new GUIStyle(Label)
            {
                fontSize = small,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                fixedHeight = Scaled(28f),
                padding = new RectOffset(Round(12f), Round(12f), 0, 0),
                margin = new RectOffset(0, 0, Round(3f), Round(3f)),
                normal = { background = Rounded(6f, Panel), textColor = TextPrimary },
                hover = { background = Rounded(6f, Raised), textColor = Color.white },
                active = { background = Rounded(6f, Accent), textColor = Color.white },
                border = Border(6f)
            };

            SmallButton = new GUIStyle(Button)
            {
                fontSize = Round(11f),
                fixedHeight = Scaled(22f),
                padding = new RectOffset(Round(8f), Round(8f), 0, 0)
            };

            DangerButton = new GUIStyle(Button)
            {
                fontStyle = FontStyle.Bold,
                normal = { background = Rounded(6f, Danger), textColor = Color.white },
                hover = { background = Rounded(6f, new Color(0.82f, 0.30f, 0.30f)), textColor = Color.white }
            };

            PillOn = new GUIStyle(Button)
            {
                fontSize = Round(11f),
                fontStyle = FontStyle.Bold,
                fixedHeight = Scaled(22f),
                padding = new RectOffset(Round(8f), Round(8f), 0, 0),
                normal = { background = Rounded(11f, Accent), textColor = Color.white },
                hover = { background = Rounded(11f, Accent), textColor = Color.white },
                border = Border(11f)
            };

            PillOff = new GUIStyle(PillOn)
            {
                normal = { background = Rounded(11f, Panel), textColor = TextMuted },
                hover = { background = Rounded(11f, Raised), textColor = TextPrimary }
            };

            TextBlock = new GUIStyle(Label)
            {
                fontSize = small,
                padding = Pad(10f),
                normal = { background = Rounded(6f, Panel), textColor = TextPrimary },
                border = Border(6f)
            };

            Slider = new GUIStyle
            {
                fixedHeight = Scaled(6f),
                margin = new RectOffset(0, 0, Round(11f), Round(11f)),
                normal = { background = Rounded(3f, Raised) },
                border = Border(3f)
            };

            SliderThumb = new GUIStyle
            {
                fixedHeight = Scaled(18f),
                fixedWidth = Scaled(18f),
                normal = { background = Rounded(9f, Accent) },
                border = Border(9f)
            };

            Overlay = new GUIStyle(Label)
            {
                fontSize = Round(13f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                padding = new RectOffset(Round(10f), Round(10f), Round(5f), Round(5f)),
                normal = { background = Rounded(6f, new Color(0f, 0f, 0f, 0.62f)), textColor = Color.white },
                border = Border(6f)
            };

            Toast = new GUIStyle(Overlay)
            {
                normal = { background = Rounded(6f, Accent), textColor = Color.white }
            };

            Handle = new GUIStyle(Label)
            {
                fontSize = Round(18f),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                normal = { background = Rounded(8f, new Color(0f, 0f, 0f, 0.62f)), textColor = Color.white },
                hover = { background = Rounded(8f, Accent), textColor = Color.white },
                border = Border(8f)
            };
        }

        public static Color ToneColor(DebugTone tone)
        {
            switch (tone)
            {
                case DebugTone.Good: return new Color(0.42f, 0.86f, 0.52f);
                case DebugTone.Warn: return new Color(0.98f, 0.75f, 0.20f);
                case DebugTone.Bad: return new Color(0.96f, 0.45f, 0.44f);
                default: return TextPrimary;
            }
        }

        public float Scaled(float value) => value * _scale;

        public int Round(float value) => Mathf.Max(1, Mathf.RoundToInt(value * _scale));

        private RectOffset Pad(float value)
        {
            var v = Round(value);
            return new RectOffset(v, v, v, v);
        }

        private RectOffset Border(float radius)
        {
            var r = Round(radius);
            return new RectOffset(r, r, r, r);
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        // Nine-slice rounded rectangle. Coverage is the distance from the pixel to the
        // shape's edge, clamped to one pixel, which is what antialiases the corners.
        private Texture2D Rounded(float radius, Color fill, Color? outline = null)
        {
            var r = Round(radius);
            var size = r * 2 + 1;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var edge = EdgeDistance(x, y, size, r);
                    var coverage = Mathf.Clamp01(edge + 0.5f);
                    var color = fill;

                    if (outline.HasValue && edge < 1f)
                    {
                        // Blend the outline over the fill by how much of this pixel the
                        // outline covers, so a 1px stroke stays a 1px stroke.
                        var strength = Mathf.Clamp01(1f - edge);
                        color = Color.Lerp(fill, outline.Value, strength * outline.Value.a);
                        color.a = fill.a + (1f - fill.a) * strength * outline.Value.a;
                    }

                    color.a *= coverage;
                    pixels[y * size + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        // A rounded rectangle with a solid accent bar down the left edge.
        private Texture2D SectionBar(Color fill)
        {
            var r = Round(6f);
            var barWidth = Round(3f);
            var size = r * 2 + 1;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var edge = EdgeDistance(x, y, size, r);
                    var color = x < barWidth ? Accent : fill;
                    color.a *= Mathf.Clamp01(edge + 0.5f);
                    pixels[y * size + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        // Signed distance from a pixel centre to the edge of a rounded square: positive
        // inside, negative outside. Clamping the pixel into the straight-edged core and
        // measuring back out handles corners and edges with the same expression.
        private static float EdgeDistance(int x, int y, int size, int radius)
        {
            var px = x + 0.5f;
            var py = y + 0.5f;
            var qx = Mathf.Clamp(px, radius, size - radius);
            var qy = Mathf.Clamp(py, radius, size - radius);
            var distance = Mathf.Sqrt((px - qx) * (px - qx) + (py - qy) * (py - qy));

            return radius - distance;
        }
    }
}
