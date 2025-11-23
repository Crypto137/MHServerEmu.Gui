using System.Runtime.InteropServices;

namespace MHServerEmu.ServerManagement
{
    public class ServerSettings
    {
        private const string ConfigFileName = "Config.ini";
        private const string ConfigOverrideFileName = "ConfigOverride.ini";

        // We use different executable names depending on the version of the game the server is for.
        private static readonly string[] ExecutableFileNames = [
            "MHServerEmu",
            "MHServerEmu2016",
            "MHServerEmu2015",
            "MHServerEmu2014",
            "MHServerEmu2013",
        ];

        public string ServerDirectory { get; }
        public string ExecutablePath { get; }
        public string ConfigPath { get; }
        public string ConfigOverridePath { get; }

        public ServerConfig Config { get; private set; }
        public ServerConfig ConfigOverride { get; private set; }

        public ServerSettings(string serverDirectory)
        {
            ServerDirectory = serverDirectory;
            ExecutablePath = GetExecutablePath(serverDirectory);
            ConfigPath = Path.Combine(serverDirectory, ConfigFileName);
            ConfigOverridePath = Path.Combine(serverDirectory, ConfigOverrideFileName);
        }

        public ServerInitializationResult Initialize()
        {
            if (File.Exists(ExecutablePath) == false)
                return ServerInitializationResult.ExecutableNotFound;

            if (File.Exists(ConfigPath) == false)
                return ServerInitializationResult.ConfigNotFound;

            try
            {
                Config = new(ConfigPath);
                Config.Load();

                ConfigOverride = new(ConfigOverridePath);
                ConfigOverride.Load();
            }
            catch (Exception)
            {
                Config = null;
                ConfigOverride = null;
                return ServerInitializationResult.ConfigParseError;
            }

            return ServerInitializationResult.Success;
        }

        public string GetOverrideOrBaseConfigValue(string section, string key)
        {
            string overrideValue = ConfigOverride.GetValue(section, key);
            if (overrideValue != null)
                return overrideValue;

            return Config.GetValue(section, key);
        }

        private static string GetExecutablePath(string serverDirectory)
        {
            foreach (string executableFileName in ExecutableFileNames)
            {
                string path;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    path = Path.Combine(serverDirectory, executableFileName);
                else
                    path = Path.Combine(serverDirectory, $"{executableFileName}.exe");

                if (File.Exists(path))
                    return path;
            }

            return string.Empty;
        }
    }
}
