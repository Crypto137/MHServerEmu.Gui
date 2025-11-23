using MHServerEmu.ServerManagement.Serialization;
using System.Diagnostics;
using System.Text.Json;

namespace MHServerEmu.ServerManagement
{
    public enum ServerInstanceState
    {
        Invalid,
        Offline,
        Pending,
        Online,
    }

    public class ServerInstance
    {
        private const int StatusRequestIntervalMS = 3000;

        private static readonly HttpClient HttpClient = new();

        private readonly object _lock = new();

        private readonly string _statusUrl;
        private readonly Dictionary<string, long> _statusDict = new();

        private Process _serverProcess;

        private CancellationTokenSource _statusRequestCts;

        public ServerSettings Settings { get; }
        public ServerInstanceState State { get; private set; } = ServerInstanceState.Offline;

        public event Action<ServerInstanceState> StateChanged;
        public event Action<string> OutputReceived;

        public ServerInstance(ServerSettings settings)
        {
            Settings = settings;

            string webAddress = settings.GetOverrideOrBaseConfigValue("WebFrontend", "Address");
            string webPort = settings.GetOverrideOrBaseConfigValue("WebFrontend", "Port");
            _statusUrl = $"http://{webAddress}:{webPort}/ServerStatus";
        }

        public void Start()
        {
            if (State != ServerInstanceState.Offline)
                return;

            string path = Settings.ExecutablePath;

            if (File.Exists(path) == false)
                return;

            Debug.Assert(_serverProcess == null);

            // Start server process
            _serverProcess = new()
            {
                StartInfo = new()
                {
                    FileName = path,
                    WorkingDirectory = Path.GetDirectoryName(path),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            _serverProcess.OutputDataReceived += ServerProcess_OutputDataReceived;
            _serverProcess.Start();
            _serverProcess.BeginOutputReadLine();

            // Start status request task
            _statusRequestCts = new();
            Task.Run(async () => await GetStatusAsync(_statusRequestCts.Token));

            SetState(ServerInstanceState.Pending);
        }

        public void Stop()
        {
            if (State == ServerInstanceState.Offline)
                return;

            Debug.Assert(_serverProcess != null);

            // Stop the status timer
            CancellationTokenSource cts = Interlocked.Exchange(ref _statusRequestCts, null);
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            // Kill the server process
            _serverProcess.OutputDataReceived -= ServerProcess_OutputDataReceived;
            _serverProcess.Kill();
            _serverProcess = null;

            // Clear status data
            _statusDict.Clear();

            SetState(ServerInstanceState.Offline);
        }

        public bool SendInput(string input)
        {
            if (State != ServerInstanceState.Online)
                return false;

            Debug.Assert(_serverProcess != null);

            _serverProcess.StandardInput.WriteLine(input);

            return true;
        }

        public long? GetStatus(string key)
        {
            lock (_lock)
            {
                if (_statusDict.TryGetValue(key, out long value) == false)
                    return null;

                return value;
            }
        }

        private void SetState(ServerInstanceState newState)
        {
            lock (_lock)
            {
                if (State == newState)
                    return;

                // kludge for potential async weirdness, need a better way to do this
                if (newState == ServerInstanceState.Online && State != ServerInstanceState.Pending)
                    return;

                State = newState;
                StateChanged?.Invoke(newState);
            }
        }

        private async Task GetStatusAsync(CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                await DoGetStatusAsync();
                await Task.Delay(StatusRequestIntervalMS, token);
            }
        }

        private async Task DoGetStatusAsync()
        {
            HttpResponseMessage response;

            try
            {
                response = await HttpClient.GetAsync(_statusUrl);
            }
            catch (HttpRequestException)
            {
                return;
            }

            if (response.IsSuccessStatusCode == false)
                return;

            Stream inputStream = await response.Content.ReadAsStreamAsync();
            Dictionary<string, long> serverStatus = await JsonSerializer.DeserializeAsync(inputStream, JsonContext.Default.DictionaryStringInt64);

            if (serverStatus == null)
                return;

            lock (_lock)
            {
                foreach (var kvp in serverStatus)
                    _statusDict[kvp.Key] = kvp.Value;
            }

            if (State == ServerInstanceState.Pending)
                SetState(ServerInstanceState.Online);
        }

        private void ServerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputReceived?.Invoke(e.Data);
        }
    }
}
