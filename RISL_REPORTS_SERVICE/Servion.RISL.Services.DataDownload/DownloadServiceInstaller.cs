using System.ComponentModel;
using System.Configuration.Install;


namespace Servion.Kbank.Services.DataDownload
{
    [RunInstaller(true)]
    public partial class DownloadServiceInstaller : Installer
    {
        public DownloadServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
