using System.Collections.Generic;

namespace LiangTools.Debugging
{
    public enum ScreenHalf
    {
        Left,
        Right
    }

    public readonly struct TapStep
    {
        public readonly ScreenHalf Half;
        public readonly int Count;

        public TapStep(ScreenHalf half, int count)
        {
            Half = half;
            Count = count;
        }
    }

    public sealed class TapGesture
    {
        public static readonly TapStep[] DefaultPattern =
        {
            new TapStep(ScreenHalf.Left, 1),
            new TapStep(ScreenHalf.Right, 2),
            new TapStep(ScreenHalf.Left, 3)
        };

        private readonly ScreenHalf[] _sequence;
        private readonly float _timeout;

        private int _matched;
        private float _lastTapTime;

        public TapGesture(IReadOnlyList<TapStep> pattern = null, float timeoutSeconds = 2f)
        {
            _sequence = Flatten(pattern ?? DefaultPattern);
            _timeout = timeoutSeconds;
        }

        public int Length => _sequence.Length;

        public int Progress => _matched;

        public void Reset()
        {
            _matched = 0;
        }

        public bool Feed(ScreenHalf half, float time)
        {
            if (_sequence.Length == 0)
            {
                return false;
            }

            if (_matched > 0 && time - _lastTapTime > _timeout)
            {
                _matched = 0;
            }

            _lastTapTime = time;

            if (_sequence[_matched] == half)
            {
                _matched++;
            }
            else
            {
                _matched = _sequence[0] == half ? 1 : 0;
            }

            if (_matched < _sequence.Length)
            {
                return false;
            }

            _matched = 0;
            return true;
        }

        private static ScreenHalf[] Flatten(IReadOnlyList<TapStep> pattern)
        {
            var total = 0;
            for (var i = 0; i < pattern.Count; i++)
            {
                total += pattern[i].Count > 0 ? pattern[i].Count : 0;
            }

            var flat = new ScreenHalf[total];
            var index = 0;
            for (var i = 0; i < pattern.Count; i++)
            {
                for (var t = 0; t < pattern[i].Count; t++)
                {
                    flat[index++] = pattern[i].Half;
                }
            }

            return flat;
        }
    }
}
