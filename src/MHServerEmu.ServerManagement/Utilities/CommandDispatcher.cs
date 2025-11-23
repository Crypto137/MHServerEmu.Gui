namespace MHServerEmu.ServerManagement.Utilities
{
    public enum CommandResult
    {
        Success,
        InvalidServerInstance,
        InvalidInput,
        InvalidArgumentGeneric,
        InvalidEmail,
        InvalidPlayerName,
        InvalidPassword,
        InvalidUserLevel,
        InputNotSent,
    }

    /// <summary>
    /// Provides a high level API for sending commands to a <see cref="ServerInstance"/>.
    /// </summary>
    public static class CommandDispatcher
    {
        private const char CommandPrefix = '!';

        /// <summary>
        /// Validates and sends the specified command to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendCommand(ServerInstance serverInstance, string command)
        {
            if (serverInstance == null || serverInstance.State != ServerInstanceState.Online)
                return CommandResult.InvalidServerInstance;

            if (string.IsNullOrWhiteSpace(command))
                return CommandResult.InvalidInput;

            if (command.StartsWith(CommandPrefix) == false)
                return CommandResult.InvalidInput;

            if (serverInstance.SendInput(command) == false)
                return CommandResult.InputNotSent;

            return CommandResult.Success;
        }

        /// <summary>
        /// Sends the server shutdown command to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendServerShutdown(ServerInstance serverInstance)
        {
            return SendCommand(serverInstance, "!server shutdown");
        }

        /// <summary>
        /// Sends the live tuning reload command to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendServerReloadLiveTuning(ServerInstance serverInstance)
        {
            return SendCommand(serverInstance, "!server reloadlivetuning");
        }

        /// <summary>
        /// Sends the store catalog reload command to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendServerReloadCatalog(ServerInstance serverInstance)
        {
            return SendCommand(serverInstance, "!server reloadcatalog");
        }

        /// <summary>
        /// Sends a server broadcast command with the specified text to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendServerBroadcast(ServerInstance serverInstance, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return CommandResult.InvalidArgumentGeneric;

            string command = $"!server broadcast {text}";
            return SendCommand(serverInstance, command);
        }

        /// <summary>
        /// Sends an account creation command with the specified arguments to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendAccountCreate(ServerInstance serverInstance, string email, string playerName, string password)
        {
            if (InputValidator.ValidateEmail(email) == false)
                return CommandResult.InvalidEmail;

            if (InputValidator.ValidatePlayerName(playerName) == false)
                return CommandResult.InvalidPlayerName;

            if (InputValidator.ValidatePassword(password) == false)
                return CommandResult.InvalidPassword;

            string command = $"!account create {email} {playerName} {password}";
            return SendCommand(serverInstance, command);
        }

        /// <summary>
        /// Sends a password change command for the specified account to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendAccountSetPassword(ServerInstance serverInstance, string email, string newPassword)
        {
            if (InputValidator.ValidateEmail(email) == false)
                return CommandResult.InvalidEmail;

            if (InputValidator.ValidatePassword(newPassword) == false)
                return CommandResult.InvalidPassword;

            string command = $"!account password {email} {newPassword}";
            return SendCommand(serverInstance, command);
        }

        /// <summary>
        /// Sends a user level change command for the specified account to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendAccountSetUserLevel(ServerInstance serverInstance, string email, int newUserLevel)
        {
            if (InputValidator.ValidateEmail(email) == false)
                return CommandResult.InvalidEmail;

            if (InputValidator.ValidateUserLevel(newUserLevel) == false)
                return CommandResult.InvalidUserLevel;

            string command = $"!account userlevel {email} {newUserLevel}";
            return SendCommand(serverInstance, command);
        }

        /// <summary>
        /// Sends a ban command for the specified account to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendAccountBan(ServerInstance serverInstance, string email)
        {
            if (InputValidator.ValidateEmail(email) == false)
                return CommandResult.InvalidEmail;

            string command = $"!account ban {email}";
            return SendCommand(serverInstance, command);
        }

        /// <summary>
        /// Sends an unban command for the specified account to the provided <see cref="ServerInstance"/>.
        /// </summary>
        public static CommandResult SendAccountUnban(ServerInstance serverInstance, string email)
        {
            if (InputValidator.ValidateEmail(email) == false)
                return CommandResult.InvalidEmail;

            string command = $"!account unban {email}";
            return SendCommand(serverInstance, command);
        }
    }
}
