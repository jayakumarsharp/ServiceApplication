using System.ComponentModel;
using System.Configuration.Install;


namespace Servion.RISL.Services.DataDownload
{
    [RunInstaller(true)]
    public partial class DataDownloadServiceInstaller : Installer
    {
        public DataDownloadServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
