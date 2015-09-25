using System;

namespace Framework.Logger
{
    /// <summary>
    ///  ILogger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        ///     Logs Debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Debug(string message);

        /// <summary>
        ///     Logs Info message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Info(string message);

        /// <summary>
        ///     Logs Trace message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Trace(string message);

        /// <summary>
        ///     Logs Warn message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Warn(string message);

        /// <summary>
        ///     Logs Error message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Error(string message);

        /// <summary>
        ///     Logs Error message and exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        void Error(string message, Exception ex);

        /// <summary>
        ///     Logs Fatal message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Fatal(string message);

        /// <summary>
        ///     Logs Fatal message and exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        void Fatal(string message, Exception ex);

        /// <summary>
        ///     Sets the custom property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void SetCustomProperty(string name, object value);
    }
}
