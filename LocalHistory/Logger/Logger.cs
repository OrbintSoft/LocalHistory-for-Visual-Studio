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

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="name">The name of this logger istance.</param>
        /// <param name="type">The type of the class that is currently using this logger.</param>
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

        public void LogError(string message, Exception exception, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            throw new NotImplementedException();
        }

        public void LogFatal(string message, Exception exception, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            throw new NotImplementedException();
        }

        public void LogMessage(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            throw new NotImplementedException();
        }

        public void Trace()
        {
            throw new NotImplementedException();
        }

        public void Trace(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            throw new NotImplementedException();
        }
    }
}
