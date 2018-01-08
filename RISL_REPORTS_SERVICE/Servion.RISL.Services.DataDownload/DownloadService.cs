using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;

namespace Servion.Kbank.Services.DataDownload
{
    public partial class DownloadService : ServiceBase
    {
        #region Constructor
        public DownloadService()
        {
            InitializeComponent();
            Thread.CurrentThread.Name = "CALLDATA_DOWNLOAD_SERVICE";
            StaticInfo.CanContinue = true;
            //OnStart(null);
        }
        #endregion

        #region Download Service Start & Stop
        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.Log.Info("Inside Method");

                if (!ConfigurationHelper.ValidateConfiguration())
                {
                    throw new ApplicationException("Could not Start the Service.. Invalid Configuration settings");
                }

                this.StartSimultaneousDownloadProcess();

                Logger.Log.Info("Data Download Service Started Successfully");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Failed to start the call data download service", ex);
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

                Logger.Log.Info("Data Download Service Stopped Successfully");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Failed to stop the transportation recovery service", ex);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// To span the threads to start the data download process based on the configuration settings
        /// </summary>
        private void StartSimultaneousDownloadProcess()
        {
            Logger.Log.Info("Inside Method");
            int threadCount = 0;
            DownloadServiceSettings downloadSettings = ConfigurationManager.GetSection("DownloadServiceSettings") as DownloadServiceSettings;
            FtpDirectorySettings ftpDirSettings = ConfigurationManager.GetSection("FtpDirectorySettings") as FtpDirectorySettings;
            foreach (FtpSetting setting in downloadSettings.FtpSettings)
            {
                if (setting.IsActive)
                {
                    DataDownload downloadUtility = new DataDownload();
                    ParameterizedThreadStart paramThread = new ParameterizedThreadStart(downloadUtility.StartDownload);
                    Thread threadInstance = new Thread(paramThread);
                    threadInstance.Name = string.Format("{0}-{1}", setting.Name, setting.VxmlServer);
                    threadInstance.Start(new object[] { setting, ftpDirSettings.DirectorySettings[setting.FtpDirectorySetting] });
                    threadCount++;
                    Interlocked.Increment(ref StaticInfo.ThreadCount);
                }
            }
            Logger.Log.InfoFormat("Total No of Thread Started : {0}", threadCount);
        }
        #endregion
    }
}
