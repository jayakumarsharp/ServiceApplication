using System.ComponentModel;
using System.Configuration.Install;


namespace Servion.RISL.Services.DataRecovery
{
    [RunInstaller(true)]
    public partial class RecoveryServiceInstaller : Installer
    {
        public RecoveryServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
