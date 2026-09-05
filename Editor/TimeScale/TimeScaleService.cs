using UnityEditor;
using UnityEngine;

namespace LiangTools.Editor.TimeControl
{
    [InitializeOnLoad]
    public static class TimeScaleService
    {
        public const float DefaultValue = 1f;

        private const string DesiredKey = "LiangTools.TimeScale.Desired";

        private static float _lastSeen;

        public static event System.Action Changed;

        static TimeScaleService()
        {
            _lastSeen = Time.timeScale;
            EditorApplication.update += PollExternalChanges;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static float Value
        {
            get => Time.timeScale;
            set => Apply(TimeScaleSettings.instance.Snap(value));
        }

        public static bool IsPaused => Mathf.Approximately(Time.timeScale, 0f);

        public static bool IsDefault => Mathf.Approximately(Time.timeScale, DefaultValue);

        public static float DesiredScale
        {
            get => EditorPrefs.GetFloat(DesiredKey, DefaultValue);
            private set => EditorPrefs.SetFloat(DesiredKey, value);
        }

        public static void Reset()
        {
            Apply(DefaultValue);
        }

        public static void SetPaused(bool paused)
        {
            Apply(paused ? 0f : ResumeValue());
        }

        public static void Nudge(int steps)
        {
            var settings = TimeScaleSettings.instance;
            var delta = settings.step > 0f ? settings.step : 0.1f;
            Apply(settings.Snap(Time.timeScale + steps * delta));
        }

        internal static void NotifyRangeChanged()
        {
            Apply(TimeScaleSettings.instance.Snap(Time.timeScale));
            Changed?.Invoke();
        }

        private static float ResumeValue()
        {
            var desired = DesiredScale;
            return desired > 0f ? desired : DefaultValue;
        }

        private static void Apply(float value)
        {
            var clamped = TimeScaleSettings.instance.Clamp(value);
            if (clamped > 0f)
            {
                DesiredScale = clamped;
            }

            if (Mathf.Approximately(Time.timeScale, clamped))
            {
                return;
            }

            Time.timeScale = clamped;
            _lastSeen = clamped;
            Changed?.Invoke();
        }

        private static void PollExternalChanges()
        {
            var current = Time.timeScale;
            if (Mathf.Approximately(current, _lastSeen))
            {
                return;
            }

            _lastSeen = current;
            Changed?.Invoke();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && TimeScaleSettings.instance.reapplyOnEnteringPlayMode)
            {
                Apply(DesiredScale);
            }
        }
    }
}
