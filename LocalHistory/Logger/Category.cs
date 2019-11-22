namespace LOSTALLOY.LocalHistory.Logger
{
    /// <summary>
    /// This enum defines a category/area of logging.
    /// </summary>
    public enum Category
    {
        /// <summary>
        /// For generic logging.
        /// </summary>
        GENERIC = 0,

        /// <summary>
        /// Related to document repository or document node.
        /// </summary>
        REPOSITORY = 1,

        /// <summary>
        /// For any background taks.
        /// </summary>
        BACKGROUND = 2,

        /// <summary>
        /// For something that depends from the viusla studio package, DTE events, etc..
        /// </summary>
        PACKAGE = 3,

        /// <summary>
        /// For anything that depends from User input or user interaction.
        /// </summary>
        USER = 4,
    }
}
