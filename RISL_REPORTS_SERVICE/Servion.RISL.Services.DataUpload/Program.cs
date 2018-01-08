using System.ServiceProcess;

namespace Servion.RISL.Services.DataUpload
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
				new UploadService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
