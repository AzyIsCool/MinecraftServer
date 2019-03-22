using System.Globalization;
using System.Windows.Controls;

namespace MinecraftServer
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            AmountOfLinesToShowTB.Text = App.Settings != null ? App.Settings.AmountOfLinesToShow.ToString() : "700";
        }

        private async void AmountOfLinesToShowTB_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var line = long.TryParse(((TextBox) sender).Text, NumberStyles.None, new NumberFormatInfo(),
                out long amountOfLines);
            if (line)
            {
                await Settings.UpdateSettings(App.Settings, "AmountOfLinesToShow", amountOfLines.ToString());
            }
        }
    }
}
