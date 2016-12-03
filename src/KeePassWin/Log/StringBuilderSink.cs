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
        private readonly KeePassSettings _settings;

        public StringBuilderSink(KeePassSettings settings)
        {
            _settings = settings;
            _sb = new StringBuilder();
            _writer = new StringWriter(_sb);

            _settings.PropertyChanged += (s, e) =>
            {
                if (string.Equals(nameof(ShouldLogEvents), e.PropertyName, StringComparison.Ordinal))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShouldLogEvents)));
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Emit(LogEvent logEvent)
        {
            if (!_settings.TrackTelemetry)
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
            get { return _settings.TrackTelemetry; }
            set { _settings.TrackTelemetry = value; }
        }
    }
}
