using System;
using System.IO;
using System.Reflection;

namespace KeePass.Win.ViewModels
{
    public class SettingsPageViewModel
    {
        private static readonly Lazy<string> s_about = new Lazy<string>(() => GetString("About.md"));
        private static readonly Lazy<string> s_version = new Lazy<string>(() => GetString("version.txt"));
        private static readonly Lazy<string> s_privacy = new Lazy<string>(() => GetString("PrivacyPolicy.md"));
        private static readonly Lazy<string> s_changelog = new Lazy<string>(() => GetString("CHANGELOG.md"));

        public SettingsPageViewModel(LoggingPageViewModel logViewModel, KeePassSettings settings, KeyboardShortcuts shortcuts)
        {
            Logging = logViewModel;
            Settings = settings;
            Shortcuts = shortcuts;
        }

        private static string GetString(string name)
        {
            var names = typeof(SettingsPageViewModel).GetTypeInfo().Assembly.GetManifestResourceNames();

            using (var stream = typeof(SettingsPageViewModel).GetTypeInfo().Assembly.GetManifestResourceStream($"KeePass.Win.ViewModels.{name}"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd().Trim();
            }
        }

        public KeePassSettings Settings { get; }

        public LoggingPageViewModel Logging { get; }

        public KeyboardShortcuts Shortcuts { get; }

        public string About => s_about.Value;

        public string Version => s_version.Value;

        public string PrivacyStatement => s_privacy.Value;

        public string Changelog => s_changelog.Value;

        public int[] TimeoutValues { get; } = new[] { 15, 30, 45, 90, 120 };
    }
}
