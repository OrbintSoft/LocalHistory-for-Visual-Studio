namespace LOSTALLOY.LocalHistory.Logger
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// This class provides a custom logger for localhistory.
    /// </summary>
    public class Logger : ILogger
    {
        private readonly NLog.Logger nlogger;

        static Logger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "localhistory.log" };
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;
        }

        public Logger(string name, Type type = null)
        {
            if (type is null)
            {
                this.nlogger = NLog.LogManager.GetLogger(name);
            }
            else
            {
                this.nlogger = NLog.LogManager.GetLogger(name, type);
            }
        }

        public static Logger Create(string name, Type type = null)
        {
            return new Logger(name, type);
        }

        public void Log(LogLevel loglevel, string message, Lazy<object> @object = null, Exception exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            
        }

        public void LogDump(string message, object @object, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            throw new NotImplementedException();
        }

        public void Trace()
        {
            throw new NotImplementedException();
        }
    }
}
