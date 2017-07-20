namespace KeePass
{
    public interface IPasswordGeneratorSettings
    {
        bool IncludeArabicNumerals { get; }

        bool IncludeLowercase { get; }

        bool IncludeOthers { get; }

        bool IncludeSpecialCharacters { get; }

        bool IncludeUppercase { get; }

        int Length { get; }

        string OtherCharacters { get; }
    }
}