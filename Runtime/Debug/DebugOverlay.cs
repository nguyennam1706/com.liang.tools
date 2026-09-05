#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
using UnityEngine;

namespace LiangTools.Debugging
{
    [AddComponentMenu("")]
    public sealed class DebugOverlay : MonoBehaviour
    {
        private const string ShowFpsKey = "LiangTools.Debug.ShowFps";
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!LiangDebug.IsAvailable || _instance != null)
            {
                return;
            }

            var host = new GameObject("[Liang Debug Overlay]") { hideFlags = HideFlags.HideAndDontSave };
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
            LiangDebug.Register(new SystemInfoPage());
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
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

            var half = current.mousePosition.x < Screen.width * 0.5f ? ScreenHalf.Left : ScreenHalf.Right;
            if (OpenGesture.Feed(half, Time.unscaledTime))
            {
                SetOpen(true);
            }
        }

        private void DrawHandle()
        {
            if (!ShowHandle)
            {
                return;
            }

            var size = _skin.Scaled(34f);
            var rect = new Rect(Screen.width - size - _skin.Scaled(8f), _skin.Scaled(8f), size, size);
            if (GUI.Button(rect, "≡", _skin.Button))
            {
                SetOpen(true);
            }
        }

        private void DrawWindow()
        {
            var margin = _skin.Scaled(12f);
            var area = new Rect(margin, margin, Screen.width - margin * 2f, Screen.height - margin * 2f);

            GUILayout.BeginArea(area, _skin.Window);

            var pages = LiangDebug.RegisteredPages;
            DrawHeader(pages.Count);

            if (pages.Count > 0)
            {
                _pageIndex = Mathf.Clamp(_pageIndex, 0, pages.Count - 1);
                DrawTabs(pages);

                _scroll = GUILayout.BeginScrollView(_scroll);
                pages[_pageIndex].Draw(_ui);
                _ui.EndSection();
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No debug pages registered.", _skin.Label);
            }

            GUILayout.EndArea();
        }

        private void DrawHeader(int pageCount)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Liang Debug</b>  ·  {pageCount} page(s)", _skin.Label);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("✕", _skin.Button, GUILayout.Width(_skin.Scaled(40f))))
            {
                SetOpen(false);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawTabs(System.Collections.Generic.IReadOnlyList<IDebugPage> pages)
        {
            GUILayout.BeginHorizontal();
            for (var i = 0; i < pages.Count; i++)
            {
                var style = i == _pageIndex ? _skin.ActiveTab : _skin.Tab;
                if (GUILayout.Button(pages[i].Title, style))
                {
                    _pageIndex = i;
                    _scroll = Vector2.zero;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawToast()
        {
            if (string.IsNullOrEmpty(_toast) || Time.unscaledTime > _toastUntil)
            {
                return;
            }

            var size = _skin.Overlay.CalcSize(new GUIContent(_toast));
            var rect = new Rect((Screen.width - size.x) * 0.5f, Screen.height - size.y - _skin.Scaled(40f), size.x, size.y);
            GUI.Label(rect, _toast, _skin.Overlay);
        }
    }
}
#endif
