using System;
using System.Security.Cryptography;
using System.Text;

namespace KeePass
{
    public class PasswordGenerator
    {
        private readonly RandomNumberGenerator _rngProvider = RandomNumberGenerator.Create();

        private const string s_uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string s_lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string s_numbers = "0123456789";
        private const string s_special = @"!@#$%^&*()_-=+[{]}\|:;'""<,>.?/~`";

        public string Generate(IPasswordGeneratorSettings settings)
        {
            var sb = new StringBuilder();

            if (settings.IncludeUppercase)
            {
                sb.Append(s_uppercase);
            }

            if (settings.IncludeLowercase)
            {
                sb.Append(s_lowercase);
            }

            if (settings.IncludeSpecialCharacters)
            {
                sb.Append(s_special);
            }

            if (settings.IncludeArabicNumerals)
            {
                sb.Append(s_numbers);
            }

            if (settings.IncludeOthers && settings.OtherCharacters != null)
            {
                sb.Append(settings.OtherCharacters);
            }

            var inputLength = sb.Length;

            if (inputLength == 0)
            {
                return null;
            }

            var result = new StringBuilder();

            for (var i = 0; i < settings.Length; i++)
            {
                var index = Next(0, inputLength);

                result.Append(sb[index]);
            }

            return result.ToString();
        }

        private int Next()
        {
            var randomBuffer = new byte[4];
            _rngProvider.GetBytes(randomBuffer);

            return BitConverter.ToInt32(randomBuffer, 0);
        }

        private int Next(int maximumValue)
        {
            // Do not use Next() % maximumValue because the distribution is not OK
            return Next(0, maximumValue);
        }

        private int Next(int minimumValue, int maximumValue)
        {
            var seed = Next();

            //  Generate uniformly distributed random integers within a given range.
            return new Random(seed).Next(minimumValue, maximumValue);
        }
    }
}
