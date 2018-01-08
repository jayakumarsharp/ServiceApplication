using System.ComponentModel;
using System.Configuration.Install;


namespace Servion.RISL.Services.DataUpload
{
    [RunInstaller(true)]
    public partial class UploadServiceInstaller : Installer
    {
        public UploadServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
