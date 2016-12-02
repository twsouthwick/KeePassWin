using System;

namespace KeePass.Win.Log
{
    internal class SerilogLogger : ILogger
    {
        private readonly Serilog.ILogger _log;

        public SerilogLogger(Serilog.ILogger log)
        {
            _log = log;
        }

        public void Debug(string format, params object[] obj)
        {
            _log.Debug(format, obj);
        }

        public void Error(string format, params object[] obj)
        {
            _log.Error(format, obj);
        }

        public void Error(Exception e, string format, params object[] obj)
        {
            _log.Error(e, format, obj);
        }

        public void Info(string format, params object[] obj)
        {
            _log.Information(format, obj);
        }

        public void Warning(string format, params object[] obj)
        {
            _log.Warning(format, obj);
        }
        public void Warning(Exception e, string format, params object[] obj)
        {
            _log.Warning(e, format, obj);
        }
    }
}
