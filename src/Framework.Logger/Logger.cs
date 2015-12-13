using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Ninject.Extensions.Logging.Log4net.Infrastructure;

namespace Framework.Logger
{
    public class Logger : Log4NetLogger, ILogger
    {
        /// <summary>
        ///     The dump verbatim format.
        /// </summary>
        private const string DumpVerbatimFormat = "{0}";
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="type"></param>
        public Logger(Type type) 
            : base(type)
        {
        }

        /// <summary>
        ///     Logs the message with Debug severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Debug(string message)
        {
            base.Debug(DumpVerbatimFormat, message);
        }

        /// <summary>
        ///     Logs the message with Info severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Info(string message)
        {
            base.Info(DumpVerbatimFormat, message);
        }

        /// <summary>
        ///     Logs the message with Trace severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Trace(string message)
        {
            base.Trace(DumpVerbatimFormat, message);
        }

        /// <summary>
        ///     Logs the message with Warn severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Warn(string message)
        {
            base.Warn(DumpVerbatimFormat, message);
        }

        /// <summary>
        ///     Logs the message with Error severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Error(string message)
        {
            base.Error(DumpVerbatimFormat, message);
        }

        /// <summary>
        ///     Logs the message with Error severity and exception information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Error(string message, Exception ex)
        {
            base.Error(ex, DumpVerbatimFormat, message);
        }

        /// <summary>
        ///     Logs the message with Fatal severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Fatal(string message)
        {
            base.Fatal(DumpVerbatimFormat, message);
        }

        /// <summary>
        ///     Logs the message with Fatal severity and exception information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Fatal(string message, Exception ex)
        {
            base.Fatal(ex, DumpVerbatimFormat, message);
        }

        /// <summary>
        ///     Sets a custom property which can then be logged.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="value">Property value.</param>
        public void SetCustomProperty(string name, object value)
        {
            ThreadContext.Properties[name] = value;
        }
    }
}
