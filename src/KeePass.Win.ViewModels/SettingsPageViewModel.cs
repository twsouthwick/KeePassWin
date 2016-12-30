using System.IO;

namespace KeePass.Win.ViewModels
{
    public class SettingsPageViewModel
    {
        public SettingsPageViewModel(LoggingPageViewModel logViewModel, KeePassSettings settings, KeyboardShortcuts shortcuts)
        {
            Logging = logViewModel;
            Settings = settings;
            Shortcuts = shortcuts;

            Version = File.ReadAllText("version.txt").Trim();
            PrivacyStatement = File.ReadAllText("PrivacyPolicy.txt");
        }

        public KeePassSettings Settings { get; }

        public LoggingPageViewModel Logging { get; }

        public KeyboardShortcuts Shortcuts { get; }

        public string Version { get; set; }

        public string PrivacyStatement { get; set; }

        public Library[] Libraries { get; } = new Library[]
        {
            new Library { Name = "Autofac", Url = "https://autofac.org/" },
            new Library { Name = "Prism", Url = "https://github.com/PrismLibrary/Prism" },
            new Library { Name = "UWP Community Toolkit", Url = "http://www.uwpcommunitytoolkit.com" }
        };

        public int[] TimeoutValues { get; } = new[] { 15, 30, 45, 90, 120 };
    }

    public class Library
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
