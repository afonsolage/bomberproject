#if _SERVER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonLib.Server
{
    public class LogLine
    {
        public Color color;
        public string message;

        public LogLine(Color color, string message)
        {
            this.color = color;
            this.message = message;
        }

        public LogLine(ref LogLine line)
        {
            Copy(ref line);
        }

        public void Copy(ref LogLine line)
        {
            color = line.color;
            message = line.message;
        }
    }

    public class LogComponent : Panel
    {
        private static readonly int MAX_LOG_LINES = 1000;

        private LogLine[] _logLines;
        private object _logLock;

        private int _lastLogLine;

        public LogComponent()
        {
            base.DoubleBuffered = true;

            _logLock = new object();
            _logLines = new LogLine[MAX_LOG_LINES];
            _tmp = new LogLine[MAX_LOG_LINES];

            for(var i = 0; i < _tmp.Length; i++)
            {
                _tmp[i] = new LogLine(Color.Red, "");
            }
        }

        private int _lastPrintedCount;
        private LogLine[] _tmp;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var count = 0;
            lock (_logLock)
            {
                count = _lastLogLine;
                for (var i = 0; i < count; i++)
                {
                    _tmp[i].Copy(ref _logLines[i]);
                }
            }

            var halfFont = Font.Height;

            if (_lastPrintedCount != _lastLogLine)
            {
                _lastPrintedCount = _lastLogLine;
                AutoScrollMinSize = new Size(AutoScrollMinSize.Width, count * halfFont);
                ScrollToBottom();
            }

            var graphics = e.Graphics;
            graphics.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);

            graphics.DrawRectangle(new Pen(BackColor), 0, 0, Size.Width, Size.Height);

            for(var i = 0; i < count; i++)
            {
                var item = _tmp[i];
                using (SolidBrush br = new SolidBrush(item.color))
                {
                    graphics.DrawString(item.message, Font, br, 0, i * halfFont);
                }
            }
        }

        public void ScrollToBottom()
        {
            using (Control c = new Control() { Parent = this, Dock = DockStyle.Bottom })
            {
                ScrollControlIntoView(c);
                c.Parent = null;
            }
        }

        internal void AddLogText(string text, Color lineColor)
        {
            var newLine = new LogLine(lineColor, text);

            lock (_logLock)
            {
                if (_lastLogLine == MAX_LOG_LINES)
                {
                    for (var i = 0; i < MAX_LOG_LINES - 1; i++)
                    {
                        _logLines[i] = _logLines[i + 1];
                    }

                    _logLines[MAX_LOG_LINES - 1] = newLine;
                }
                else
                {
                    _logLines[_lastLogLine++] = newLine;
                }
            }

            Invalidate();
        }

        public void Clear()
        {
            lock(_logLock)
            {
                _lastLogLine = 0;
                Invalidate();
            }
        }
    }
}

#endif