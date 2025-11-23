namespace MHServerEmu.ServerManagement
{
    public enum ServerInitializationResult
    {
        Success,
        ExecutableNotFound,
        ConfigNotFound,
        ConfigParseError,
    }

    public class ServerManager
    {
        public ServerInstance CurrentInstance { get; private set; }

        public ServerManager()
        {
        }

        public ServerInitializationResult Initialize(string serverDirectory)
        {
            ServerSettings settings = new(serverDirectory);

            ServerInitializationResult result = settings.Initialize();
            if (result != ServerInitializationResult.Success)
                return result;

            CurrentInstance = new(settings);

            return ServerInitializationResult.Success;
        }
    }
}
