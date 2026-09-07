using UnityEngine;

namespace LiangTools.Debugging
{
    internal sealed class DebugSkin
    {
        private const float ReferenceDpi = 160f;

        private readonly float _scale;

        public GUIStyle Window { get; }
        public GUIStyle Section { get; }
        public GUIStyle SectionBody { get; }
        public GUIStyle Label { get; }
        public GUIStyle Key { get; }
        public GUIStyle Value { get; }
        public GUIStyle Button { get; }
        public GUIStyle SmallButton { get; }
        public GUIStyle Toggle { get; }
        public GUIStyle Tab { get; }
        public GUIStyle ActiveTab { get; }
        public GUIStyle Slider { get; }
        public GUIStyle SliderThumb { get; }
        public GUIStyle Overlay { get; }
        public GUIStyle TableHeader { get; }
        public GUIStyle Cell { get; }
        public GUIStyle TextBlock { get; }
        public GUIStyle DangerButton { get; }
        public Texture2D RowHighlight { get; }
        public Texture2D RowStripe { get; }

        public DebugSkin()
        {
            var dpi = Screen.dpi > 1f ? Screen.dpi : ReferenceDpi;
            _scale = Mathf.Clamp(dpi / ReferenceDpi, 1f, 3.5f);

            var body = Mathf.RoundToInt(13f * _scale);

            Window = new GUIStyle(GUI.skin.box)
            {
                padding = Pad(8),
                normal = { background = Solid(new Color(0.09f, 0.09f, 0.11f, 0.96f)) }
            };

            Label = new GUIStyle(GUI.skin.label) { fontSize = body, wordWrap = true, richText = true };
            Key = new GUIStyle(Label) { fixedWidth = Scaled(190f) };
            Value = new GUIStyle(Label) { alignment = TextAnchor.MiddleLeft };

            Section = new GUIStyle(GUI.skin.button)
            {
                fontSize = body,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = Pad(6),
                normal = { background = Solid(new Color(1f, 1f, 1f, 0.07f)), textColor = Color.white }
            };

            SectionBody = new GUIStyle { padding = new RectOffset(Round(10f), 0, Round(2f), Round(6f)) };

            Button = new GUIStyle(GUI.skin.button) { fontSize = body, padding = Pad(6) };
            SmallButton = new GUIStyle(Button) { fontSize = Mathf.RoundToInt(body * 0.85f), padding = Pad(3) };
            Toggle = new GUIStyle(GUI.skin.toggle) { fontSize = body };

            Tab = new GUIStyle(Button) { fontStyle = FontStyle.Normal };
            ActiveTab = new GUIStyle(Button)
            {
                fontStyle = FontStyle.Bold,
                normal = { background = Solid(new Color(0.25f, 0.5f, 0.9f, 0.9f)), textColor = Color.white }
            };

            Slider = new GUIStyle(GUI.skin.horizontalSlider) { fixedHeight = Scaled(14f) };
            SliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                fixedHeight = Scaled(18f),
                fixedWidth = Scaled(18f)
            };

            TableHeader = new GUIStyle(Label)
            {
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                normal = { textColor = new Color(0.68f, 0.72f, 0.78f) }
            };

            Cell = new GUIStyle(Label) { wordWrap = false, clipping = TextClipping.Clip };

            TextBlock = new GUIStyle(Label)
            {
                padding = Pad(6),
                normal = { background = Solid(new Color(1f, 1f, 1f, 0.05f)), textColor = Color.white }
            };

            DangerButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = body,
                fontStyle = FontStyle.Bold,
                padding = Pad(6),
                normal = { background = Solid(new Color(0.62f, 0.19f, 0.19f, 0.95f)), textColor = Color.white }
            };

            RowHighlight = Solid(new Color(0.25f, 0.5f, 0.9f, 0.35f));
            RowStripe = Solid(new Color(1f, 1f, 1f, 0.035f));

            Overlay = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(14f * _scale),
                fontStyle = FontStyle.Bold,
                padding = Pad(4),
                normal = { background = Solid(new Color(0f, 0f, 0f, 0.55f)), textColor = Color.white }
            };
        }

        public static Color ToneColor(DebugTone tone)
        {
            switch (tone)
            {
                case DebugTone.Good: return new Color(0.45f, 0.85f, 0.5f);
                case DebugTone.Warn: return new Color(0.98f, 0.78f, 0.32f);
                case DebugTone.Bad: return new Color(0.95f, 0.45f, 0.42f);
                default: return Color.white;
            }
        }

        public float Scaled(float value) => value * _scale;

        public int Round(float value) => Mathf.RoundToInt(value * _scale);

        private RectOffset Pad(float value)
        {
            var v = Round(value);
            return new RectOffset(v, v, v, v);
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
