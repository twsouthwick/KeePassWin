using Serilog.Core;
using Serilog.Events;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace KeePass.Win.Log
{
    internal class StringBuilderSink : ILogEventSink, INotifyPropertyChanged, ILogView
    {
        private readonly StringBuilder _sb;
        private readonly StringWriter _writer;

        private bool _shouldLogEvents;

        public StringBuilderSink()
        {
            _sb = new StringBuilder();
            _writer = new StringWriter(_sb);

#if DEBUG
            _shouldLogEvents = true;
#endif
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Emit(LogEvent logEvent)
        {
            if (!ShouldLogEvents)
            {
                return;
            }

            _writer.Write($"[{logEvent.Level} {logEvent.Timestamp}] ");
            logEvent.RenderMessage(_writer);

            if (logEvent.Exception != null)
            {
                _writer.WriteLine(logEvent.Exception.ToString());
            }

            _writer.WriteLine();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Log)));
        }

        public string Id { get; } = Guid.NewGuid().ToString();

        public string Log => _sb.ToString();

        public bool ShouldLogEvents
        {
            get { return _shouldLogEvents; }
            set
            {
                if (_shouldLogEvents == value)
                {
                    return;
                }

                _shouldLogEvents = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShouldLogEvents)));
            }
        }
    }
}
