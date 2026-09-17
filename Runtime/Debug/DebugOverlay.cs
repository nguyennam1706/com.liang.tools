#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
using UnityEngine;

namespace LiangTools.Debugging
{
    [AddComponentMenu("")]
    public sealed class DebugOverlay : MonoBehaviour
    {
        private const string ShowFpsKey = "LiangTools.Debug.ShowFps";
        private const float CornerWidthRatio = 0.25f;
        private const float CornerHeightRatio = 0.15f;
        private const string ShowHandleKey = "LiangTools.Debug.ShowHandle";

        private static DebugOverlay _instance;

        private DebugSkin _skin;
        private DebugUi _ui;
        private Vector2 _scroll;
        private int _pageIndex;
        private string _toast;
        private float _toastUntil;

        public static DebugOverlay Instance => _instance;

        public bool IsOpen { get; private set; }

        public FpsCounter Fps { get; } = new FpsCounter();

        public TapGesture OpenGesture { get; } = new TapGesture();

        public bool ShowHandle
        {
            get => PlayerPrefs.GetInt(ShowHandleKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(ShowHandleKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public bool ShowFpsOverlay
        {
            get => PlayerPrefs.GetInt(ShowFpsKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(ShowFpsKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForDomainReload()
        {
            _instance = null;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!LiangDebug.IsAvailable || _instance != null)
            {
                return;
            }

            // HideInHierarchy only: HideFlags.HideAndDontSave carries DontSaveInEditor,
            // and Unity does not clean those up when Play mode ends, so the host
            // survived into edit mode and another one was created on every run.
            var host = new GameObject("[Liang Debug Overlay]") { hideFlags = HideFlags.HideInHierarchy };
            DontDestroyOnLoad(host);
            host.AddComponent<DebugOverlay>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            LiangDebug.Register(new FpsPage());
            LiangDebug.Register(new LogPage());
            LiangDebug.Register(new SystemInfoPage());
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void DestroySelf()
        {
            IsOpen = false;

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                // Destroy() is a no-op outside Play mode and logs a warning.
                DestroyImmediate(gameObject);
            }
        }

        public void SetOpen(bool open)
        {
            IsOpen = open;
            OpenGesture.Reset();
        }

        public void Toast(string message, float seconds = 1.5f)
        {
            _toast = message;
            _toastUntil = Time.unscaledTime + seconds;
        }

        private void Update()
        {
            Fps.Sample(Time.unscaledDeltaTime);
        }

        private void DetectOpenGesture(Event current)
        {
            if (current == null || current.type != EventType.MouseDown)
            {
                return;
            }

            if (!TryResolveCorner(current.mousePosition, out var corner))
            {
                // A tap anywhere else is ignored rather than treated as a miss, so
                // ordinary play does not constantly break a half-finished sequence.
                return;
            }

            if (OpenGesture.Feed(corner, Time.unscaledTime))
            {
                SetOpen(true);
            }
        }

        // IMGUI coordinates start at the top-left, so the top band is small y.
        private static bool TryResolveCorner(Vector2 position, out ScreenCorner corner)
        {
            corner = default;

            if (position.y > Screen.height * CornerHeightRatio)
            {
                return false;
            }

            if (position.x <= Screen.width * CornerWidthRatio)
            {
                corner = ScreenCorner.TopLeft;
                return true;
            }

            if (position.x >= Screen.width * (1f - CornerWidthRatio))
            {
                corner = ScreenCorner.TopRight;
                return true;
            }

            return false;
        }

        private void OnGUI()
        {
            // Belt and braces: if a host from an earlier session ever outlives Play
            // mode again, it removes itself instead of drawing over the editor.
            if (!Application.isPlaying)
            {
                DestroySelf();
                return;
            }

            _skin ??= new DebugSkin();
            _ui ??= new DebugUi(_skin);

            if (!IsOpen)
            {
                DetectOpenGesture(Event.current);
                DrawFpsOverlay();
                DrawHandle();
                return;
            }

            DrawWindow();
            DrawToast();
        }

        private void DrawFpsOverlay()
        {
            if (!ShowFpsOverlay)
            {
                return;
            }

            var text = $"{Fps.Current:0} fps · {Fps.FrameTimeMs:0.0} ms";
            var size = _skin.Overlay.CalcSize(new GUIContent(text));
            var rect = new Rect(_skin.Scaled(10f), _skin.Scaled(10f), size.x, size.y);

            using (new GuiColorScope(DebugSkin.ToneColor(RateTone(Fps.Current))))
            {
                GUI.Label(rect, text, _skin.Overlay);
            }
        }

        private void DrawHandle()
        {
            if (!ShowHandle)
            {
                return;
            }

            var size = _skin.Scaled(38f);
            var rect = new Rect(Screen.width - size - _skin.Scaled(10f), _skin.Scaled(10f), size, size);
            if (GUI.Button(rect, "≡", _skin.Handle))
            {
                SetOpen(true);
            }
        }

        private void DrawWindow()
        {
            // Dim the game behind the panel: over a bright scene the translucent panel
            // alone left the text hard to read.
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none, _skin.Scrim);

            var margin = _skin.Scaled(14f);
            var area = new Rect(margin, margin, Screen.width - margin * 2f, Screen.height - margin * 2f);

            GUILayout.BeginArea(area, GUIContent.none, _skin.Window);

            var pages = LiangDebug.RegisteredPages;
            DrawHeader(pages);

            if (pages.Count > 0)
            {
                _pageIndex = Mathf.Clamp(_pageIndex, 0, pages.Count - 1);
                DrawTabs(pages, area.width - _skin.Scaled(28f));

                _scroll = GUILayout.BeginScrollView(_scroll, GUIStyle.none, GUI.skin.verticalScrollbar);
                pages[_pageIndex].Draw(_ui);
                _ui.EndSection();
                GUILayout.Space(_skin.Scaled(8f));
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No debug pages registered.", _skin.Key);
            }

            GUILayout.EndArea();
        }

        private void DrawHeader(System.Collections.Generic.IReadOnlyList<IDebugPage> pages)
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Liang Debug", _skin.Title);
            GUILayout.Label(
                $"{Fps.Current:0} fps · {Fps.FrameTimeMs:0.0} ms · {pages.Count} pages · {Application.version}",
                _skin.Subtitle);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("✕", _skin.Close, GUILayout.Width(_skin.Scaled(30f))))
            {
                SetOpen(false);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(_skin.Scaled(6f));
        }

        // Tabs wrap onto further rows rather than shrinking, so a project that
        // registers a dozen pages stays legible on a phone.
        private void DrawTabs(System.Collections.Generic.IReadOnlyList<IDebugPage> pages, float available)
        {
            var used = 0f;
            var open = false;

            for (var i = 0; i < pages.Count; i++)
            {
                var badge = pages[i].Badge;
                var label = string.IsNullOrEmpty(badge)
                    ? pages[i].Title
                    : $"{pages[i].Title}  {badge}";

                var style = i == _pageIndex ? _skin.ActiveTab : _skin.Tab;
                var width = style.CalcSize(new GUIContent(label)).x + _skin.Scaled(4f);

                if (!open || used + width > available)
                {
                    if (open)
                    {
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    open = true;
                    used = 0f;
                }

                if (GUILayout.Button(label, style, GUILayout.Width(width)))
                {
                    _pageIndex = i;
                    _scroll = Vector2.zero;
                }

                used += width;
            }

            if (open)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(_skin.Scaled(4f));
        }

        private void DrawToast()
        {
            if (string.IsNullOrEmpty(_toast) || Time.unscaledTime > _toastUntil)
            {
                return;
            }

            var size = _skin.Toast.CalcSize(new GUIContent(_toast));
            var rect = new Rect((Screen.width - size.x) * 0.5f, Screen.height - size.y - _skin.Scaled(48f), size.x, size.y);
            GUI.Label(rect, _toast, _skin.Toast);
        }

        private static DebugTone RateTone(float fps)
        {
            if (fps <= 0f)
            {
                return DebugTone.Normal;
            }

            return fps >= 55f ? DebugTone.Good : fps >= 28f ? DebugTone.Warn : DebugTone.Bad;
        }

        private readonly struct GuiColorScope : System.IDisposable
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
    }
}
#endif
