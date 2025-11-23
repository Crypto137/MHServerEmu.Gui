using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MHServerEmu.Gui.Avalonia.Views.Dialogs;
using MHServerEmu.ServerManagement;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MHServerEmu.Gui.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        private readonly StringBuilder _outputBuilder = new();
        private readonly ServerManager _serverManager;

        private ServerInstance _serverInstance;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(ServerManager serverManager) : this()
        {
            _serverManager = serverManager;
        }

        private async void Initialize()
        {
            const string ServerDirectory = "MHServerEmu";

            // Create a server directory if it doesn't already exist to make it more obvious where to put the server build for the user.
            if (Directory.Exists(ServerDirectory) == false)
                Directory.CreateDirectory(ServerDirectory);

            ServerInitializationResult result = _serverManager.Initialize(ServerDirectory);
            SetServerInstance(_serverManager.CurrentInstance);

            if (result != ServerInitializationResult.Success)
            {
                await MessageBoxWindow.Show(this, $"Failed to initialize MHServerEmu ({result}).", "Error");
                Environment.Exit(0);
            }
        }

        private void SetServerInstance(ServerInstance serverInstance)
        {
            if (_serverInstance != null)
            {
                _serverInstance.StateChanged -= ServerInstance_StateChanged;
                _serverInstance.OutputReceived -= ServerInstance_OutputReceived;
                _serverInstance.Stop();
            }

            _serverInstance = serverInstance;

            if (_serverInstance != null)
            {
                _serverInstance.StateChanged += ServerInstance_StateChanged;
                _serverInstance.OutputReceived += ServerInstance_OutputReceived;

                ServerInstance_StateChanged(_serverInstance.State);
            }
            else
            {
                ServerInstance_StateChanged(ServerInstanceState.Invalid);
            }
        }

        #region Event Handlers

        private void StartStopServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serverInstance == null)
                return;

            switch (_serverInstance.State)
            {
                case ServerInstanceState.Offline:
                    _outputBuilder.Clear();
                    _serverInstance.Start();
                    break;

                case ServerInstanceState.Pending:
                case ServerInstanceState.Online:
                    _serverInstance.Stop();
                    break;
            }
        }

        private void CommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serverInstance == null)
                return;

            CommandWindow commandWindow = new(_serverInstance);
            commandWindow.ShowDialog(this);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: SettingsWindow should open ServerConfigWindow if server is available
            if (_serverInstance == null)
                return;

            ServerConfigWindow settingsWindow = new(_serverInstance.Settings);
            settingsWindow.ShowDialog(this);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _serverInstance?.Stop();
            Environment.Exit(0);
        }

        private void Window_Opened(object sender, EventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            Initialize();
        }

        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            _serverInstance?.Stop();
        }

        // ServerInstance events may not be coming from the UI thread, so they need to be wrapped in Dispatcher.UIThread.Post().

        private void ServerInstance_StateChanged(ServerInstanceState newState)
        {
            Dispatcher.UIThread.Post(() =>
            {
                switch (newState)
                {
                    case ServerInstanceState.Invalid:
                        ServerStatusTextBlock.Text = "Not Available";
                        StartStopServerButton.Content = "Start Server";
                        StartStopServerButton.IsEnabled = false;
                        CommandButton.IsEnabled = false;
                        SettingsButton.IsEnabled = false;
                        break;

                    case ServerInstanceState.Offline:
                        ServerStatusTextBlock.Text = "Offline";
                        StartStopServerButton.Content = "Start Server";
                        StartStopServerButton.IsEnabled = true;
                        CommandButton.IsEnabled = false;
                        SettingsButton.IsEnabled = true;
                        break;

                    case ServerInstanceState.Pending:
                        ServerStatusTextBlock.Text = "Starting...";
                        StartStopServerButton.Content = "Stop Server";
                        StartStopServerButton.IsEnabled = true;
                        CommandButton.IsEnabled = false;
                        SettingsButton.IsEnabled = false;
                        break;

                    case ServerInstanceState.Online:
                        ServerStatusTextBlock.Text = "Online";
                        StartStopServerButton.Content = "Stop Server";
                        StartStopServerButton.IsEnabled = true;
                        CommandButton.IsEnabled = true;
                        SettingsButton.IsEnabled = false;
                        break;
                }
            });
        }

        private void ServerInstance_OutputReceived(string output)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Debug.Assert(_serverInstance != null);

                _outputBuilder.AppendLine(output);
                ServerOutputTextBox.Text = _outputBuilder.ToString();
                ServerOutputTextBox.CaretIndex = int.MaxValue;
            });
        }

        #endregion
    }
}