using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MinecraftServer
{
    /// <summary>
    /// Interaction logic for ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        private string Type = "";
        private string FileLocation = "";
        private Logger LogFile;
        private long LineCount = 0;
        private Process ServerProcess
        {
            get
            {
                foreach (var process in App.Processes)
                {
                    if (process.StartInfo.FileName == FileLocation ||
                        process.StartInfo.Arguments.Contains(FileLocation))
                    { return process; }
                }
                return null;
            }
        }
        private bool RestartServer = false;
        private OpenFileDialog openFileDialog = null;

        public ServerPage(string type, string fileLocation = "")
        {
            Startup(type, fileLocation);
        }

        private async Task Startup(string type, string fileLocation = "")
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Server Programs (*.exe, *.jar)|*.exe;*.jar"
            };
            Type = type;
            Title = $"{Type} Server";
            if (!string.IsNullOrWhiteSpace(fileLocation)) { await AddProcess(fileLocation); }
            FileLocation = fileLocation;
            if (!string.IsNullOrWhiteSpace(fileLocation)) { await _FileOrArgs(); }
            await UpdateProgramLocationGUI(fileLocation);
        }

        #region Triggers
        private async void ServerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                if(LogFile == null)
                    LogFile = new Logger(Type + "Server (" + DateTime.Now.ToShortDateString().Replace("/", "") + DateTime.Now.ToLongTimeString().Replace(":", "") + ").log");
                await Dispatcher.InvokeAsync(() => ProcessText.Text += e.Data + Environment.NewLine);
                LogFile.Log(e.Data);
                LineCount++;
                await RemoveLines();
            }
        }

        private async void ChangeServerProgramLocation_OnClick(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog().Value)
            {
                await AddProcess(openFileDialog.FileName);
                await UpdateProgramLocationGUI(openFileDialog.FileName);
            }
        }

        private async void SendTextToProcess_OnClick(object sender, RoutedEventArgs e)
        {
            if (TextToSendToProcess.Text.Length > 0)
            {
                ProcessText.Text += TextToSendToProcess.Text + Environment.NewLine;
                LogFile.Log(TextToSendToProcess.Text);
                ServerProcess.StandardInput.WriteLine(TextToSendToProcess.Text);
                TextToSendToProcess.Text = "";
                LineCount++;
                await RemoveLines();
            }
        }

        private void StartProcess_OnClick(object sender, RoutedEventArgs e)
        {
            ProcessText.Text = "";
            LineCount = 0;
            LogFile = new Logger(Type + "Server (" + DateTime.Now.ToShortDateString().Replace("/", "") + DateTime.Now.ToLongTimeString().Replace(":", "") + ").log");
            ServerProcess.Start();
            ServerProcess.BeginOutputReadLine();
            TextToSendToProcess.IsEnabled = true;
            SendTextToProcess.IsEnabled = true;
            StartProcess.IsEnabled = false;
            EndProcess.IsEnabled = true;
            RestartProcess.IsEnabled = true;
        }

        private async void ServerProcess_Exited(object sender, EventArgs e)
        {
            ServerProcess.CancelOutputRead();
            LogFile.Dispose();
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                TextToSendToProcess.IsEnabled = false;
                SendTextToProcess.IsEnabled = false;
            });
            if (RestartServer)
            {
                await App.Current.Dispatcher.InvokeAsync(() => ProcessText.Text = "" );
                LogFile = new Logger(Type + "Server (" + DateTime.Now.ToShortDateString().Replace("/", "") + DateTime.Now.ToLongTimeString().Replace(":", "") + ").log");
                ServerProcess.Start();
                ServerProcess.BeginOutputReadLine();
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    TextToSendToProcess.IsEnabled = true;
                    SendTextToProcess.IsEnabled = true;
                    StartProcess.IsEnabled = false;
                    EndProcess.IsEnabled = true;
                    RestartProcess.IsEnabled = true;
                });
            }
            else
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    StartProcess.IsEnabled = true;
                    EndProcess.IsEnabled = false;
                    RestartProcess.IsEnabled = false;
                });
            }
            RestartServer = false;
        }

        private async void EndProcess_OnClick(object sender, RoutedEventArgs e)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                EndProcess.IsEnabled = false;
                RestartProcess.IsEnabled = false;
            });
            ServerProcess.StandardInput.WriteLine("stop");
        }

        private async void RestartProcess_OnClick(object sender, RoutedEventArgs e)
        {
            RestartServer = true;
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                EndProcess.IsEnabled = false;
                RestartProcess.IsEnabled = false;
            });
            ServerProcess.StandardInput.WriteLine("stop");
        }

        private void TextToSendToProcess_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendTextToProcess.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        #endregion

        #region Logic
        private async Task RemoveLines()
        {
            if (LineCount > App.Settings.AmountOfLinesToShow)
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (!string.IsNullOrWhiteSpace(ProcessText.Text))
                    {
                        var wew = ProcessText.Text.IndexOf(Environment.NewLine) + Environment.NewLine.Length;
                        ProcessText.Text = ProcessText.Text.Remove(0, wew);
                        LineCount--;
                    }
                });

            }
        }

        async Task _FileOrArgs()
        {
            var sp = ServerProcess;
            if (FileLocation.Split('.').Last() == "jar")
            {
                sp.StartInfo.Arguments = $"-jar \"{FileLocation}\"";
                sp.StartInfo.FileName = "java";
            }
            else
            {
                sp.StartInfo.FileName = FileLocation;
                sp.StartInfo.Arguments = "";
            }
            sp.StartInfo.WorkingDirectory = await App.GetDir(FileLocation);
        }

        private async Task AddProcess(string fileLocation)
        {
            if (!string.IsNullOrWhiteSpace(FileLocation))
            {
                await _FileOrArgs();
            }
            else
            {
                var serverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        FileName = fileLocation
                    },
                    EnableRaisingEvents = true
                };
                serverProcess.Exited += ServerProcess_Exited;
                serverProcess.OutputDataReceived += ServerProcess_OutputDataReceived;
                App.Processes.Add(serverProcess);
                FileLocation = fileLocation;
                await _FileOrArgs();
            }
            await Settings.UpdateSettings(App.Settings, Type, fileLocation);
        }

        private async Task UpdateProgramLocationGUI(string fileLocation)
        {
            ServerProgramLocation.Text = $"{Type} Server Location: {fileLocation}";
            FileLocation = fileLocation;
            StartProcess.IsEnabled = !string.IsNullOrWhiteSpace(fileLocation);
        }
        #endregion
    }
}
