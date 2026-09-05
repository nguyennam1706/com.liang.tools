using UnityEngine;

namespace LiangTools.Debugging
{
    public sealed class FpsCounter
    {
        private readonly float[] _samples;

        private int _count;
        private int _next;
        private float _accumulated;

        public FpsCounter(int sampleCount = 120)
        {
            _samples = new float[Mathf.Max(1, sampleCount)];
            Reset();
        }

        public float Current { get; private set; }

        public float Average { get; private set; }

        public float Minimum { get; private set; }

        public float Maximum { get; private set; }

        public float FrameTimeMs => Current > 0f ? 1000f / Current : 0f;

        public void Reset()
        {
            _count = 0;
            _next = 0;
            _accumulated = 0f;
            Current = 0f;
            Average = 0f;
            Minimum = float.MaxValue;
            Maximum = 0f;
        }

        public void Sample(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            Current = 1f / deltaTime;

            if (_count == _samples.Length)
            {
                _accumulated -= _samples[_next];
            }
            else
            {
                _count++;
            }

            _samples[_next] = Current;
            _accumulated += Current;
            _next = (_next + 1) % _samples.Length;

            Average = _accumulated / _count;
            Minimum = Mathf.Min(Minimum, Current);
            Maximum = Mathf.Max(Maximum, Current);
        }
    }
}
