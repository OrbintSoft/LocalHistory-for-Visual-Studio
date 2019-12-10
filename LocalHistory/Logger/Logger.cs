namespace LOSTALLOY.LocalHistory.Logger
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// This class provides a custom logger for localhistory.
    /// </summary>
    public class Logger : ILogger
    {
        private readonly NLog.Logger nlogger;
        private readonly string name;
        private readonly Type type;
        private readonly Category category;

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
        /// <param name="category">The category.</param>
        private Logger(string name, Type type = null, Category category = Category.GENERIC)
        {
            this.name = name;
            this.type = type;
            this.category = category;

            if (type is null)
            {
                this.nlogger = NLog.LogManager.GetLogger(name);
            }
            else
            {
                this.nlogger = NLog.LogManager.GetLogger(name, type);
            }
        }

        /// <summary>
        /// This method creates a new instance of <see cref="Logger"/>.
        /// </summary>
        /// <param name="name">A name for the logger.</param>
        /// <param name="type">The type of the class that is using the logger.</param>
        /// <param name="category">A log category.</param>
        /// <returns>An istance of logger.</returns>
        public static Logger Create(string name, Type type = null, Category category = Category.GENERIC)
        {
            return new Logger(name, type, category);
        }

        /// <inheritdoc/>
        public void Log(LogLevel loglevel, string message, Lazy<object> @object = null, Exception exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            // TODO: improve log
            Debug.WriteLine(message);
            this.nlogger.Info(message);
            if (LocalHistoryPackage.Instance != null)
            {
                LocalHistoryPackage.Log(message);
            }
        }

        /// <inheritdoc/>
        public void LogDump(string message, Lazy<object> @object, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.Log(LogLevel.DEBUG, message, @object, null, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <inheritdoc/>
        public void LogError(string message, Exception exception, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.Log(LogLevel.ERROR, message, null, exception, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <inheritdoc/>
        public void LogFatal(string message, Exception exception, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.Log(LogLevel.FATAL, message, null, exception, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <inheritdoc/>
        public void LogMessage(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.Log(LogLevel.INFORMATION, message, null, null, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.Log(LogLevel.WARNING, message, null, null, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <inheritdoc/>
        public void Trace(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.Log(LogLevel.TRACE, message, null, null, memberName, sourceFilePath, sourceLineNumber);
        }
    }
}
