using Ninject.Activation;
using System;
using System.Collections.Generic;

namespace Framework.Logger
{
    /// <summary>
    ///     Logger Factory
    /// </summary>
    public class LoggerFactory
    {
        /// <summary>
        ///     The type to logger map
        /// </summary>
        private static readonly Dictionary<Type, ILogger> TypeToLoggerMap = new Dictionary<Type, ILogger>();

        /// <summary>
        ///     Gets the logger.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static ILogger GetLogger(IContext context)
        {
            return GetLogger(context.Request.Target == null 
                ? typeof (ILogger) 
                : context.Request.Target.Member.DeclaringType);
        }

        /// <summary>
        ///     Gets the logger    
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static ILogger GetLogger(Type type)
        {
            lock (TypeToLoggerMap)
            {
                if (TypeToLoggerMap.ContainsKey(type)) return TypeToLoggerMap[type];

                ILogger logger = new Logger(type);
                TypeToLoggerMap.Add(type, logger);

                return logger;
            }
        }
    }
}
