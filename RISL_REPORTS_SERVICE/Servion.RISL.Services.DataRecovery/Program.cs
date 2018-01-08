using System.ServiceProcess;

namespace Servion.RISL.Services.DataRecovery
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new RecoveryService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
