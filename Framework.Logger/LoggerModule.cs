using Ninject.Modules;
using log4net.Config;

namespace Framework.Logger
{
    /// <summary>
    ///     Ninject logger module. Register the logger factory.
    /// </summary>
    public class LoggerModule : NinjectModule
    {
        public override void Load()
        {
            XmlConfigurator.Configure();
            Bind<ILogger>().ToMethod(LoggerFactory.GetLogger);
        }
    }
}
