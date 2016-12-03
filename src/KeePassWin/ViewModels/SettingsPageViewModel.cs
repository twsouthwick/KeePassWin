using System.IO;

namespace KeePass.Win.ViewModels
{
    public class SettingsPageViewModel
    {
        public SettingsPageViewModel(LoggingPageViewModel logViewModel, KeePassSettings settings)
        {
            Logging = logViewModel;
            Settings = settings;

            Version = File.ReadAllText("version.txt").Trim();
            PrivacyStatement = File.ReadAllText("PrivacyPolicy.txt");
        }

        public KeePassSettings Settings { get; }

        public LoggingPageViewModel Logging { get; }

        public string Version { get; set; }

        public string PrivacyStatement { get; set; }

        public Library[] Libraries { get; } = new Library[]
        {
            new Library { Name = "Autofac", Url = "https://autofac.org/" },
            new Library { Name = "Prism", Url = "https://github.com/PrismLibrary/Prism" },
            new Library { Name = "UWP Community Toolkit", Url = "http://www.uwpcommunitytoolkit.com" }
        };
    }

    public class Library
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
