#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
using UnityEngine.Scripting;

namespace LiangTools.Debugging
{
    /// <summary>
    /// Reads the session log in game: filter by type, tap a row for the full message
    /// and stack, copy the whole log out.
    /// </summary>
    [Preserve]
    public sealed class LogPage : IDebugPage
    {
        private static readonly string[] Headers = { "", "Time", "Message" };

        private bool _showLog = true;
        private bool _showWarning = true;
        private bool _showError = true;

        // Row whose detail is open; negative means nothing is selected.
        private int _selected = -1;

        public string Title => "Logs";

        // Between FPS (0) and System (10): the log is what gets opened most.
        public int Order => 5;

        public string Badge => ResolveBadge();

        public void Draw(DebugUi ui)
        {
            if (ui.Section("Capture"))
            {
                var capturing = ui.Toggle("Capturing", DebugLogStore.IsCapturing);
                if (capturing != DebugLogStore.IsCapturing)
                {
                    DebugLogStore.SetCapturing(capturing);
                    _selected = -1;
                }

                ui.Row("State", ReadCaptureHint(),
                    DebugLogStore.IsCapturing ? DebugTone.Good : DebugTone.Normal);
            }

            if (ui.Section("Counts"))
            {
                ui.Row("Held", $"{DebugLogStore.Count}/{DebugLogStore.Capacity}");
                ui.Row("Errors", DebugLogStore.ErrorCount.ToString(),
                    DebugLogStore.ErrorCount > 0 ? DebugTone.Bad : DebugTone.Good);
                ui.Row("Warnings", DebugLogStore.WarningCount.ToString(),
                    DebugLogStore.WarningCount > 0 ? DebugTone.Warn : DebugTone.Good);
            }

            if (ui.Section("Filter"))
            {
                SetFilter(ref _showLog, ui.Toggle("Log", _showLog));
                SetFilter(ref _showWarning, ui.Toggle("Warning", _showWarning));
                SetFilter(ref _showError, ui.Toggle("Error", _showError));
            }

            if (ui.Section("Log"))
            {
                var rows = DebugLogStore.GetRows(_showLog, _showWarning, _showError);
                var clicked = ui.Table(Headers, rows, _selected, 34f, 76f);

                if (clicked >= 0)
                {
                    _selected = clicked == _selected ? -1 : clicked;
                }
            }

            if (ui.Section("Detail"))
            {
                ui.TextBlock(ReadDetail());

                // Copying is the point of the detail view: a stack trace is what gets
                // pasted into a bug report, and it cannot be selected on a phone.
                ui.Copy("Copy full details", _selected < 0 ? null : DebugLogStore.GetDetail(_selected));
            }

            if (ui.Section("Manage"))
            {
                ui.Copy("Copy the whole log", DebugLogStore.Dump());

                if (ui.Button("Clear", "Discard every held line?"))
                {
                    DebugLogStore.Clear();
                    _selected = -1;
                }
            }

            ui.EndSection();
        }

        // Errors and warnings show on the tab, so something going wrong is visible
        // without opening the page. Errors win, being the thing to look at first.
        private static string ResolveBadge()
        {
            if (DebugLogStore.ErrorCount > 0)
            {
                return $"{DebugLogStore.ErrorCount} err";
            }

            return DebugLogStore.WarningCount > 0
                ? $"{DebugLogStore.WarningCount} warn"
                : null;
        }

        // Says how far back the log reaches: enabled mid-session it only has lines
        // from that point, and only the next run captures from startup.
        private static string ReadCaptureHint()
        {
            if (!DebugLogStore.IsCapturing)
            {
                return "off — nothing is being captured";
            }

            return DebugLogStore.IsCapturingFromStart
                ? "on — capturing since startup"
                : "on — only lines since you enabled it; next run captures from startup";
        }

        private string ReadDetail()
        {
            if (_selected < 0)
            {
                return "Tap a row above to see it in full.";
            }

            var detail = DebugLogStore.GetDetail(_selected);

            return string.IsNullOrEmpty(detail) ? "That line is no longer held." : detail;
        }

        // Changing a filter drops the selection: the index counts against the filtered
        // list, so a different filter makes it point at another line.
        private void SetFilter(ref bool filter, bool value)
        {
            if (filter == value)
            {
                return;
            }

            filter = value;
            _selected = -1;
        }
    }
}
#endif
