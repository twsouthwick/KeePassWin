using System.IO;

namespace KeePass.Win.ViewModels
{
    public class SettingsPageViewModel
    {
        public SettingsPageViewModel()
        {
            Version = File.ReadAllText("version.txt").Trim();
            PrivacyStatement = File.ReadAllText("PrivacyPolicy.txt");
        }

        public string Version { get; set; }

        public string PrivacyStatement { get; set; }

        public Library[] Libraries { get; } = new Library[]
        {
            new Library { Name = "Autofac", Url = "https://autofac.org/" },
            new Library { Name = "Newtonsoft.Json", Url = "http://www.newtonsoft.com/json" },
            new Library { Name = "Prism", Url = "https://github.com/PrismLibrary/Prism" },
            new Library { Name = "WinRTXamlToolkit", Url = "https://github.com/xyzzer/WinRTXamlToolkit" }
        };
    }

    public class Library
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
