using Windows.ApplicationModel.Resources;

namespace KeePass.Win.ViewModels
{
    internal class LocalizedStrings
    {
        private static readonly ResourceLoader s_resourceLoader;

        static LocalizedStrings()
        {
            s_resourceLoader = ResourceLoader.GetForCurrentView("LocalizedStrings");
        }

        /// <summary>
        /// Localized resource similar to "Invalid credentials"
        /// </summary>
        public static string InvalidCredentials
        {
            get
            {
                return s_resourceLoader.GetString("InvalidCredentials");
            }
        }

        /// <summary>
        /// Localized resource similar to "Could not open file"
        /// </summary>
        public static string MenuItemOpenError
        {
            get
            {
                return s_resourceLoader.GetString("MenuItemOpenError");
            }
        }

        /// <summary>
        /// Localized resource similar to "The database you selected is already available."
        /// </summary>
        public static string MenuItemOpenSameFileContent
        {
            get
            {
                return s_resourceLoader.GetString("MenuItemOpenSameFileContent");
            }
        }

        /// <summary>
        /// Localized resource similar to "Database already open!"
        /// </summary>
        public static string MenuItemOpenSameFileTitle
        {
            get
            {
                return s_resourceLoader.GetString("MenuItemOpenSameFileTitle");
            }
        }
    }
}
