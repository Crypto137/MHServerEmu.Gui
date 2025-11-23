using System.Text.RegularExpressions;

namespace MHServerEmu.ServerManagement.Utilities
{
    /// <summary>
    /// Validates user input for various things.
    /// </summary>
    public static partial class InputValidator
    {
        private const int EmailMaxLength = 320;
        private const int PasswordMinLength = 3;
        private const int PasswordMaxLength = 64;
        private const int UserLevelMax = 2;

        /// <summary>
        /// Returns <see langword="true"/> if the provided email <see cref="string"/> is valid.
        /// </summary>
        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (email.Length > EmailMaxLength)
                return false;

            // Validate like the client does on the login screen.
            int atIndex = email.IndexOf('@');
            if (atIndex == -1)
                return false;

            int dotIndex = email.LastIndexOf('.');
            if (dotIndex < atIndex)
                return false;

            int topDomainLength = email.Length - (dotIndex + 1);
            if (topDomainLength < 2)
                return false;

            return true;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the provided player name <see cref="string"/> is valid.
        /// </summary>
        public static bool ValidatePlayerName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return false;

            if (GetPlayerNameRegex().Match(playerName).Success == false)
                return false;

            return true;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the provided password <see cref="string"/> is valid.
        /// </summary>
        public static bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < PasswordMinLength)
                return false;

            if (password.Length > PasswordMaxLength)
                return false;

            return true;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the provided user level value is valid.
        /// </summary>
        public static bool ValidateUserLevel(int userLevel)
        {
            if (userLevel < 0)
                return false;

            if (userLevel > UserLevelMax)
                return false;

            return true;
        }

        [GeneratedRegex(@"^[a-zA-Z0-9]{1,16}$")]    // 1-16 alphanumeric characters
        private static partial Regex GetPlayerNameRegex();
    }
}
