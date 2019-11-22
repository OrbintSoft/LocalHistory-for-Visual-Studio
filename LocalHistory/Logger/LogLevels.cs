namespace LOSTALLOY.LocalHistory.Logger
{
    /// <summary>
    /// Defines a set of log levels.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// To log a detailed set of inforamtions.
        /// </summary>
        VERBOSE = 1,

        /// <summary>
        /// To log infomations useful only for debug purposes.
        /// </summary>
        DEBUG = 2,

        /// <summary>
        /// To trace the execution of a piece of a routine.
        /// </summary>
        TRACE = 3,

        /// <summary>
        /// To log useful informations.
        /// </summary>
        INFORMATION = 4,

        /// <summary>
        /// To log something important.
        /// </summary>
        IMPORTANT = 5,

        /// <summary>
        /// To log a warning.
        /// </summary>
        WARNING = 6,

        /// <summary>
        /// To log an error.
        /// </summary>
        ERROR = 7,

        /// <summary>
        /// To log a catastrofic failure.
        /// </summary>
        FATAL = 8,

        /// <summary>
        /// Your last message.
        /// </summary>
        DEATH = 9,
    }
}
