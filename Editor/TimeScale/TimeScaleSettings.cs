using UnityEditor;
using UnityEngine;

namespace LiangTools.Editor.TimeControl
{
    [FilePath("ProjectSettings/LiangToolsTimeScale.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class TimeScaleSettings : ScriptableSingleton<TimeScaleSettings>
    {
        public const float AbsoluteMax = 100f;

        public float minimum = 0f;
        public float maximum = 2f;
        public float step = 0.5f;
        public bool reapplyOnEnteringPlayMode = true;

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, minimum, maximum);
        }

        public float Snap(float value)
        {
            if (step <= 0f)
            {
                return Clamp(value);
            }

            var steps = Mathf.Round((value - minimum) / step);
            return Clamp(minimum + steps * step);
        }

        public void Persist()
        {
            minimum = Mathf.Clamp(minimum, 0f, AbsoluteMax);
            maximum = Mathf.Clamp(maximum, minimum + 0.01f, AbsoluteMax);
            step = Mathf.Clamp(step, 0f, maximum - minimum);
            Save(true);
            TimeScaleService.NotifyRangeChanged();
        }
    }
}
