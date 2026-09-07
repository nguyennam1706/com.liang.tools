using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LiangTools.Debugging
{
    /// <summary>
    /// Keeps the most recent <see cref="Capacity"/> lines Unity logged during the
    /// session. Off by default: with capture disabled no listener is registered, so a
    /// normal player's log calls cost nothing. The choice is remembered, so once
    /// enabled the next run captures from startup.
    /// </summary>
    public static class DebugLogStore
    {
        /// <summary>Lines kept; the oldest is dropped on overflow.</summary>
        public const int Capacity = 300;

        /// <summary>Most rows handed to the table in one read.</summary>
        public const int MaxRows = 150;

        private const string CaptureKey = "LiangTools.Debug.LogCapture";

        // Logs can arrive from threads other than the main one, so every read and
        // write goes through this lock.
        private static readonly object Gate = new object();

        // Fixed ring buffer: writes land at _head and wrap, so an overflowing line
        // does not shift the whole array the way List.RemoveAt(0) would.
        private static readonly DebugLogEntry[] Entries = new DebugLogEntry[Capacity];
        private static int _head;
        private static int _count;

        // The table's rows plus a pool of row arrays to reuse: the page re-reads
        // every repaint, so allocating fresh arrays each time is hot garbage.
        private static readonly List<string[]> Rows = new List<string[]>();
        private static readonly List<string[]> RowPool = new List<string[]>();

        // Snapshot of the last read, so a row index the user taps resolves to the
        // same entry even if newer lines arrived meanwhile.
        private static readonly List<DebugLogEntry> Snapshot = new List<DebugLogEntry>();

        // Timestamp text for the most recent second built. Lines within the same
        // second reuse it, so formatting runs once per second instead of per line —
        // and only on read, never on the capture path.
        private static long _stampSecond = -1;
        private static string _stampText = string.Empty;

        private static int _errorCount;
        private static int _warningCount;

        /// <summary>Whether lines are being captured.</summary>
        public static bool IsCapturing { get; private set; }

        /// <summary>
        /// The remembered choice, which is what decides whether the next run captures
        /// from startup.
        /// </summary>
        public static bool IsCaptureRemembered => PlayerPrefs.GetInt(CaptureKey, 0) == 1;

        /// <summary>
        /// This session captured from startup. False when the switch was flipped
        /// mid-session, in which case the log is missing its beginning.
        /// </summary>
        public static bool IsCapturingFromStart { get; private set; }

        /// <summary>Lines currently held.</summary>
        public static int Count
        {
            get { lock (Gate) { return _count; } }
        }

        /// <summary>Errors logged over the whole session, including dropped lines.</summary>
        public static int ErrorCount
        {
            get { lock (Gate) { return _errorCount; } }
        }

        /// <summary>Warnings logged over the whole session.</summary>
        public static int WarningCount
        {
            get { lock (Gate) { return _warningCount; } }
        }

        /// <summary>
        /// Lines matching the filter, newest first, at most <see cref="MaxRows"/>. The
        /// returned list is an internal buffer and is only valid until the next call.
        /// </summary>
        public static IList<string[]> GetRows(bool showLog, bool showWarning, bool showError)
        {
            lock (Gate)
            {
                Rows.Clear();
                Snapshot.Clear();

                for (var offset = 0; offset < _count && Rows.Count < MaxRows; offset++)
                {
                    var entry = Entries[IndexFromNewest(offset)];

                    if (!Passes(entry.Type, showLog, showWarning, showError))
                    {
                        continue;
                    }

                    Snapshot.Add(entry);
                    Rows.Add(TakeRow(entry));
                }

                return Rows;
            }
        }

        /// <summary>
        /// The full text of row <paramref name="index"/> from the last read; empty when
        /// the index has gone stale.
        /// </summary>
        public static string GetDetail(int index)
        {
            lock (Gate)
            {
                if (index < 0 || index >= Snapshot.Count)
                {
                    return string.Empty;
                }

                var entry = Snapshot[index];

                return string.IsNullOrEmpty(entry.Stack)
                    ? entry.Message
                    : $"{entry.Message}\n\n{entry.Stack}";
            }
        }

        /// <summary>Every held line as one string, for copying out.</summary>
        public static string Dump()
        {
            lock (Gate)
            {
                var builder = new StringBuilder();

                for (var offset = _count - 1; offset >= 0; offset--)
                {
                    var entry = Entries[IndexFromNewest(offset)];

                    builder.Append(ResolveStamp(entry.Ticks)).Append(' ')
                        .Append(entry.Mark).Append(' ');

                    if (entry.Repeat > 1)
                    {
                        builder.Append('x').Append(entry.Repeat).Append(' ');
                    }

                    builder.AppendLine(entry.Message);
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Starts or stops capture and remembers the choice for later runs. Enabling
        /// mid-session only captures from that point on; disabling clears the log so no
        /// strings are held.
        /// </summary>
        public static void SetCapturing(bool value)
        {
            PlayerPrefs.SetInt(CaptureKey, value ? 1 : 0);
            PlayerPrefs.Save();

            if (value)
            {
                Subscribe();
                return;
            }

            Unsubscribe();
            Clear();
        }

        /// <summary>Drops every held line; the session counters reset with it.</summary>
        public static void Clear()
        {
            lock (Gate)
            {
                Array.Clear(Entries, 0, Entries.Length);
                _head = 0;
                _count = 0;
                Snapshot.Clear();
                Rows.Clear();
                _errorCount = 0;
                _warningCount = 0;
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
        // Start of each run: listen only when the remembered choice is on. Unsubscribe
        // first because a domain reload may leave the previous session's registration
        // in place. Gated so a build without the debug define never touches the ring
        // buffer, and therefore never allocates it.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            Unsubscribe();
            Clear();
            IsCapturingFromStart = false;

            if (!IsCaptureRemembered)
            {
                return;
            }

            Subscribe();
            IsCapturingFromStart = true;
        }
#endif

        // Detaches before attaching, so calling this repeatedly cannot double-register.
        private static void Subscribe()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
            Application.logMessageReceivedThreaded += HandleLog;
            IsCapturing = true;
        }

        private static void Unsubscribe()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
            IsCapturing = false;
        }

        // The capture path: no string formatting, no timezone-aware clock read, and a
        // repeated line only bumps a counter instead of taking another slot.
        private static void HandleLog(string message, string stack, LogType type)
        {
            lock (Gate)
            {
                if (TryRepeatNewest(message, type))
                {
                    return;
                }

                Entries[_head] = new DebugLogEntry(message, stack, type);
                _head = (_head + 1) % Capacity;

                if (_count < Capacity)
                {
                    _count++;
                }

                CountByType(type);
            }
        }

        // A line identical to the newest one only increments that line's counter. Code
        // logging every frame therefore does not flush the whole log, and allocates no
        // new entry.
        private static bool TryRepeatNewest(string message, LogType type)
        {
            if (_count == 0)
            {
                return false;
            }

            var index = IndexFromNewest(0);
            var newest = Entries[index];

            if (newest.Type != type || !string.Equals(newest.Message, message))
            {
                return false;
            }

            newest.Repeat++;
            Entries[index] = newest;
            CountByType(type);

            return true;
        }

        private static void CountByType(LogType type)
        {
            if (type == LogType.Warning)
            {
                _warningCount++;
            }
            else if (type != LogType.Log)
            {
                _errorCount++;
            }
        }

        // Ring buffer slot of the line <paramref name="offset"/> back from the newest.
        private static int IndexFromNewest(int offset)
        {
            return (_head - 1 - offset + Capacity * 2) % Capacity;
        }

        // Timestamp text for a tick count, reusing the previous second's string.
        private static string ResolveStamp(long ticks)
        {
            var second = ticks / TimeSpan.TicksPerSecond;

            if (second != _stampSecond)
            {
                _stampSecond = second;
                _stampText = new DateTime(ticks).ToString("HH:mm:ss");
            }

            return _stampText;
        }

        // One table row, taken from the pool of built arrays and overwritten.
        private static string[] TakeRow(in DebugLogEntry entry)
        {
            var index = Rows.Count;

            while (RowPool.Count <= index)
            {
                RowPool.Add(new string[3]);
            }

            var row = RowPool[index];

            row[0] = entry.Repeat > 1 ? $"{entry.Mark}{entry.Repeat}" : entry.Mark;
            row[1] = ResolveStamp(entry.Ticks);
            row[2] = entry.Head;

            return row;
        }

        private static bool Passes(LogType type, bool showLog, bool showWarning, bool showError)
        {
            if (type == LogType.Log)
            {
                return showLog;
            }

            return type == LogType.Warning ? showWarning : showError;
        }

        // One logged line. Only Repeat changes after construction, so a repeated line
        // accumulates into itself.
        private struct DebugLogEntry
        {
            public LogType Type;
            public string Message;
            public string Stack;

            // Written as ticks; turning it into text is left to read time, off the
            // capture path.
            public long Ticks;

            // Type marker, taken from string constants so it does not allocate.
            public string Mark;

            // First line of the message, trimmed so the table does not stretch.
            public string Head;

            // How many identical lines arrived in a row.
            public int Repeat;

            public DebugLogEntry(string message, string stack, LogType type)
            {
                Type = type;
                Message = message ?? string.Empty;

                // Plain logs keep no stack: they are most of the lines, almost nobody
                // reads their stack, and holding 300 of them keeps several hundred KB
                // of strings from being collected.
                Stack = type == LogType.Log ? string.Empty : stack ?? string.Empty;
                Ticks = DateTime.Now.Ticks;
                Mark = ResolveMark(type);
                Head = ResolveHead(Message);
                Repeat = 1;
            }

            private static string ResolveMark(LogType type)
            {
                switch (type)
                {
                    case LogType.Log: return "i";
                    case LogType.Warning: return "!";
                    default: return "X";
                }
            }

            // First line of the message, capped at HeadLength characters. Short
            // single-line messages — most logs — return themselves without allocating.
            private static string ResolveHead(string message)
            {
                const int headLength = 70;

                var end = message.IndexOf('\n');

                if (end < 0)
                {
                    return message.Length <= headLength
                        ? message
                        : message.Substring(0, headLength) + "…";
                }

                return end <= headLength
                    ? message.Substring(0, end)
                    : message.Substring(0, headLength) + "…";
            }
        }
    }
}
