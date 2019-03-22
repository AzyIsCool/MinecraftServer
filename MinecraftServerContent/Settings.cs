using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;

namespace MinecraftServer
{
    public class Settings
    {
        public static async Task WriteSettings(Settings settings)
        {
            File.WriteAllLines("Settings.ini", new string[] { $"Java={settings.JavaServerExe}", $"Bedrock={settings.BedrockServerExe}", $"AmountOfLinesToShow={settings.AmountOfLinesToShow}" } );
        }

        public static async Task<Settings> ReadSettings()
        {
            if (File.Exists("Settings.ini"))
            {
                var settingsFile = File.ReadAllLines("Settings.ini");
                if (settingsFile != null && settingsFile.Length > 0)
                {
                    Settings settings = new Settings();
                    settings.JavaServerExe = settingsFile[0].Replace("Java=", "");
                    settings.BedrockServerExe = settingsFile[1].Replace("Bedrock=", "");
                    var gotLines = long.TryParse(settingsFile[2].Replace("AmountOfLinesToShow=", ""), NumberStyles.None, new NumberFormatInfo(), out long amountOfLinesToShow);
                    if (gotLines)
                    {
                        settings.AmountOfLinesToShow = amountOfLinesToShow;
                    }
                    return settings;
                }
            }

            return null;
        }

        public static async Task<Settings> UpdateSettings(Settings settings, string whatToUpdate, string content)
        {
            if (settings == null) { settings = new Settings(); App.Settings = settings; }
            switch (whatToUpdate)
            {
                case "Java":
                    settings.JavaServerExe = content;
                    break;
                case "Bedrock":
                    settings.BedrockServerExe = content;
                    break;
                case "AmountOfLinesToShow":
                    var canParse = long.TryParse(content, NumberStyles.None, new NumberFormatInfo(), out long amountOfLinesToShow);
                    if (canParse) settings.AmountOfLinesToShow = amountOfLinesToShow;
                    break;
            }
            await WriteSettings(settings);
            return settings;
        }

        public string JavaServerExe { get; set; }
        public string BedrockServerExe { get; set; }
        public long AmountOfLinesToShow { get; set; } = 700;
    }
}
