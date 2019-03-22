using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MinecraftServer
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private MainWindow Window;

        public MainPage(MainWindow window)
        {
            Window = window;
            InitializeComponent();

            JavaFrame.Content = new ServerPage("Java", App.Settings?.JavaServerExe);
            BedrockFrame.Content = new ServerPage("Bedrock", App.Settings?.BedrockServerExe);
            SettingsFrame.Content = new SettingsPage();
            ChangeWindowText();
        }

        private async Task ChangeWindowText(string title = "")
        {
            if (IsInitialized)
                Window.Title = !string.IsNullOrWhiteSpace(title) ? title : nameof(MinecraftServer);
            Window.tbTitle.Text = Window.Title;
        }

        private async void ServerTabs_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
                await ChangeWindowText(
                    $"{nameof(MinecraftServer)} - {((Page)((Frame)ServerTabs.SelectedContent).Content).Title}");
        }
    }
}
