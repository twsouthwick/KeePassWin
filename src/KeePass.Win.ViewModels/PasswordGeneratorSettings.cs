using KeePass.Win.ViewModels;

namespace KeePass
{
    public class PasswordGeneratorSettings : ViewModelBase, IPasswordGeneratorSettings
    {
        private bool _includeUppercase = true;
        private bool _includeLowercase = true;
        private bool _includeArabicNumerals = true;
        private bool _includeSpecialCharacters = false;
        private bool _includeOthers = false;
        private string _otherCharacters = string.Empty;
        private int _length = 12;

        public bool IncludeUppercase
        {
            get { return _includeUppercase; }
            set { SetProperty(ref _includeUppercase, value); }
        }

        public bool IncludeLowercase
        {
            get { return _includeLowercase; }
            set { SetProperty(ref _includeLowercase, value); }
        }

        public bool IncludeArabicNumerals
        {
            get { return _includeArabicNumerals; }
            set { SetProperty(ref _includeArabicNumerals, value); }
        }

        public bool IncludeSpecialCharacters
        {
            get { return _includeSpecialCharacters; }
            set { SetProperty(ref _includeSpecialCharacters, value); }
        }

        public bool IncludeOthers
        {
            get { return _includeOthers; }
            set { SetProperty(ref _includeOthers, value); }
        }

        public string OtherCharacters
        {
            get { return _otherCharacters; }
            set { SetProperty(ref _otherCharacters, value); }
        }

        public int Length
        {
            get { return _length; }
            set { SetProperty(ref _length, value); }
        }
    }
}
