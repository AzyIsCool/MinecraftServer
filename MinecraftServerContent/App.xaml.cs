using System;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MinecraftServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<Process> Processes = new List<Process>();
        public static Settings Settings = null;

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            foreach (var process in Processes)
            {
                bool processLoaded = false;
                try { processLoaded = !process.HasExited; }
                catch (Exception exception) { Console.WriteLine(exception); }

                if (processLoaded)
                {
                    process.StandardInput.WriteLine("stop");
                    process.WaitForExit();
                }
            }
        }

        public static async Task<string> GetDir(string fileLocation)
        {
            return !string.IsNullOrWhiteSpace(fileLocation) ? fileLocation.Remove(fileLocation.LastIndexOf('\\')) : "";
        }

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            Settings = await Settings.ReadSettings();
        }
    }
}
