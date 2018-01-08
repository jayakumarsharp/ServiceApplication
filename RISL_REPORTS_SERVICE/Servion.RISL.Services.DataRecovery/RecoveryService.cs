using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;

namespace Servion.RISL.Services.DataRecovery
{
    public partial class RecoveryService : ServiceBase
    {
        #region Constructor
        public RecoveryService()
        {
            InitializeComponent();
            Thread.CurrentThread.Name = ConfigurationManager.AppSettings["ApplicationName"];
            StaticInfo.CanContinue = true;
            //OnStart(null);
        }
        #endregion

        #region Recovery Service Start & Stop
        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.Log.Info("Inside Method");

                if (!ConfigurationHelper.ValidateConfiguration())
                {
                    throw new ApplicationException("Could not start the service... Invalid configuration settings");
                }

                this.EncryptConnectionStringSection();

                this.InitializeConfigurationValues();

                this.StartSimultaneousRecoveryProcess();

                Logger.Log.Info("Data Recovery Service Successfully Started");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Failed to start the data recovery service", ex);
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
                    Logger.Log.InfoFormat("There are {0} threads Which are active... Waiting for those threads to abort ", Interlocked.Read(ref StaticInfo.ThreadCount));
                }
                Logger.Log.Info("Data Recovery Service Successfully Stopped");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Failed to stop the transportation recovery service", ex);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// To store the configuration values to static members
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

            StaticInfo.FileRecoveryMaxTime = Convert.ToInt32(ConfigurationManager.AppSettings["FileRecoveryMaxTime"]);
            Logger.Log.InfoFormat("File Recovery Max Time : {0} ", StaticInfo.FileRecoveryMaxTime);

            StaticInfo.CommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["CommandTimeout"]);
            Logger.Log.InfoFormat("Database Command Timeout : {0} ", StaticInfo.CommandTimeout);

            Logger.Log.Info("Configuration Values Initialized");
        }

        /// <summary>
        /// To span the threads to start the recovery process
        /// </summary>
        private void StartSimultaneousRecoveryProcess()
        {
            Logger.Log.Info("Inside Methosd");
            int threadCount = 0;
            
            DataRecoveryServiceSettings recoverySettings = ConfigurationManager.GetSection("DataRecoveryServiceSettings") as DataRecoveryServiceSettings;

            foreach (FileReaderSetting readerSetting in recoverySettings.FileReaderSettings)
            {
                if (readerSetting.IsActive)
                {
                    threadCount++;
                    FileReader recoverFileReader = new FileReader();
                    ParameterizedThreadStart paramThread = new ParameterizedThreadStart(recoverFileReader.StartRecovery);
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
