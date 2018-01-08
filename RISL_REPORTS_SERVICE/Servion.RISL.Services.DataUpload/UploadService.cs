using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;

namespace Servion.RISL.Services.DataUpload
{
    public partial class UploadService : ServiceBase
    {
        #region Constructor
        public UploadService()
        {
            InitializeComponent();
            Thread.CurrentThread.Name = ConfigurationManager.AppSettings["ApplicationName"];
            StaticInfo.CanContinue = true;
            //OnStart(null);
        }
        #endregion

        #region Data Upload Service Start & Stop
        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.Log.Info("Inside Method");

                if (!ConfigurationHelper.ValidateConfiguration())
                {
                    throw new ApplicationException("Could not Start the Service... Invalid configuration settings");
                }

                this.InitializeConfigurationValues();

                this.EncryptConnectionStringSection();

                this.StartSimultaneousUploadProcess();

                Logger.Log.Info("Data Upload Started Successfully");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error occured while starting the service", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                StaticInfo.CanContinue = false;
                while (Interlocked.Read(ref StaticInfo.ThreadCount) > 0)
                {
                    Thread.Sleep(2000);
                    Logger.Log.InfoFormat("There are {0} threads which are active... Waiting for those threads to abort ", Interlocked.Read(ref StaticInfo.ThreadCount));
                }
                Logger.Log.Info("Data Upload Stopped Successfully");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error occured while stopping the service", ex);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// To store the configuration values to StaticInfo class members
        /// </summary>
        private void InitializeConfigurationValues()
        {
            Logger.Log.Info("Inside Methosd");

            StaticInfo.ApplicationName = ConfigurationManager.AppSettings["ApplicationName"];
            Logger.Log.InfoFormat("Application Name {0} ", StaticInfo.ApplicationName);

            if (Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(A => A.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Count() == 0)
            {
                StaticInfo.ApplicationServer = Dns.GetHostName();
            }
            else
            {
                StaticInfo.ApplicationServer = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(A => A.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First().ToString();
            }
            Logger.Log.InfoFormat("Application Server : {0} ", StaticInfo.ApplicationServer);

            StaticInfo.CommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["CommandTimeout"]);
            Logger.Log.InfoFormat("Database Command Timeout : {0} ", StaticInfo.CommandTimeout);

            StaticInfo.TempXmlPacketFolder = ConfigurationManager.AppSettings["TempXmlPacketFolder"];
            Logger.Log.InfoFormat("TempXmlPacketFolder : {0} ", StaticInfo.TempXmlPacketFolder);

            Logger.Log.Info("Configuration Values Initialized");
        }

        /// <summary>
        /// To span the threads to start the data upload process based on the configuration settings
        /// </summary>
        private void StartSimultaneousUploadProcess()
        {
            Logger.Log.Info("Inside Methosd");
            int threadCount = 0;

            DataUploadServiceSettings uploadSettings = ConfigurationManager.GetSection("DataUploadServiceSettings") as DataUploadServiceSettings;

            foreach (DataReaderSetting readerSetting in uploadSettings.DataReaderSettings)
            {
                if (readerSetting.IsActive)
                {
                    threadCount++;
                    DataReader dataReader = new DataReader();
                    ParameterizedThreadStart paramThread = new ParameterizedThreadStart(dataReader.StartUpload);
                    Thread threadInstance = new Thread(paramThread);
                    threadInstance.Name = readerSetting.Name;
                    threadInstance.Start(readerSetting);
                    Interlocked.Increment(ref StaticInfo.ThreadCount);
                }
            }

            Logger.Log.InfoFormat("Total No of Thread Started for Data Recovery : {0}", threadCount);
        }

        /// <summary>
        /// To encrypt the connectionStrings section in the configuration file
        /// </summary>
        private void EncryptConnectionStringSection()
        {
            Logger.Log.Info("Inside Methosd");
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            ConfigurationSection section = config.GetSection("connectionStrings");

            if (section != null && !section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                config.Save();
            }
        }
        #endregion
    }
}
