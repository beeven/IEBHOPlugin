using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IEPlugin
{
    [RunInstaller(true), ComVisible(false)]
    public partial class BHOInstaller : System.Configuration.Install.Installer
    {
        public BHOInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            RegistrationServices regsrv = new RegistrationServices();
            if(!regsrv.RegisterAssembly(this.GetType().Assembly, AssemblyRegistrationFlags.SetCodeBase))
            {
                throw new InstallException("Failed to register.");
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            RegistrationServices regsrv = new RegistrationServices();
            if(!regsrv.UnregisterAssembly(this.GetType().Assembly))
            {
                throw new InstallException("Failed to unregister.");
            }
        }
    }
}
