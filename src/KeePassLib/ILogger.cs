using System;

namespace KeePass
{
    public interface ILogger
    {
        void Info(string format, params object[] obj);

        void Warning(string format, params object[] obj);

        void Warning(Exception e, string format, params object[] obj);

        void Debug(string format, params object[] obj);

        void Error(string format, params object[] obj);

        void Error(Exception e, string format, params object[] obj);
    }
}
