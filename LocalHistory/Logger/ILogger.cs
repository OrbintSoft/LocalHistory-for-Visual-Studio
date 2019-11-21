namespace LOSTALLOY.LocalHistory.Logger
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// This interface is used as logger provider for LocalHistory.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// This method is used to log a message.
        /// </summary>
        /// <param name="loglevel">The level of log that you want to use.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="object">An object to dumped.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="memberName">The caller member (method or property) name.</param>
        /// <param name="sourceFilePath">The full path of the source file.</param>
        /// <param name="sourceLineNumber">The line number of the caller.</param>
        void Log(
            LogLevel loglevel,
            string message,
            Lazy<object> @object = null,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        /// <summary>
        /// This is used to log the dump of an object for debugging purpose.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="object">The object.</param>
        /// <param name="memberName">The caller member (method or property) name.</param>
        /// <param name="sourceFilePath">The full path of the source file.</param>
        /// <param name="sourceLineNumber">The line number of the caller.</param>
        void LogDump(
            string message,
            object @object,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);


        void Trace();
    }
}
