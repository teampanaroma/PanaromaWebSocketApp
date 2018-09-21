using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Panaroma.Communication.Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AllowMultipleApplication(((IEnumerable<string>)e.Args).Any() && bool.Parse(e.Args[0]));
            base.OnStartup(e);
        }

        public static void AllowMultipleApplication(bool flag = false)
        {
            if(flag)
            {
                Process.Start(ResourceAssembly.Location);
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                int openedAppId = 0;
                string assemblyName = ResourceAssembly.GetName().Name;
                Process process = Process.GetProcessesByName(assemblyName).FirstOrDefault();
                if(process != null)
                    openedAppId = process.Id;
                if(((IEnumerable<Process>)Process.GetProcesses()).Count(e => e.ProcessName.Equals(assemblyName)) <= 1)
                    return;
                int num = (int)MessageBox.Show("Uygulama zaten çalışıyor...", assemblyName, MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                Process.GetCurrentProcess().Kill();
                Task.Delay(4000);
                if(openedAppId.Equals(0))
                    return;
                ((IEnumerable<Process>)Process.GetProcessesByName(assemblyName)).Where(e => e.Id != openedAppId)
                    .ToList().ForEach(p => p.Kill());
            }
        }
    }
}