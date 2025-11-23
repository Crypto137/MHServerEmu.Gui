using Avalonia.Controls;
using Avalonia.Interactivity;
using MHServerEmu.Gui.Avalonia.Views.Dialogs;
using MHServerEmu.ServerManagement;
using MHServerEmu.ServerManagement.Utilities;
using System;
using System.Collections.Generic;

namespace MHServerEmu.Gui.Avalonia.Views;

public partial class CommandWindow : Window
{
    private readonly ServerInstance _serverInstance;

    public CommandWindow()
    {
        InitializeComponent();
    }

    public CommandWindow(ServerInstance serverInstance) : this()
    {
        _serverInstance = serverInstance;
    }

    private async void HandleCommandResult(CommandResult result)
    {
        if (result != CommandResult.Success)
        {
            await MessageBoxWindow.Show(this, $"Failed to execute command ({result}).", "Error");
            return;
        }

        Close();
    }

    #region Event Handlers

    private void ServerShutdownButton_Click(object sender, RoutedEventArgs e)
    {
        CommandResult commandResult = CommandDispatcher.SendServerShutdown(_serverInstance);
        HandleCommandResult(commandResult);
    }

    private void ServerReloadLiveTuningButton_Click(object sender, RoutedEventArgs e)
    {
        CommandResult commandResult = CommandDispatcher.SendServerReloadLiveTuning(_serverInstance);
        HandleCommandResult(commandResult);
    }

    private void ServerReloadCatalogButton_Click(object sender, RoutedEventArgs e)
    {
        CommandResult commandResult = CommandDispatcher.SendServerReloadCatalog(_serverInstance);
        HandleCommandResult(commandResult);
    }

    private async void ServerBroadcastButton_Click(object sender, RoutedEventArgs e)
    {
        (DialogResult dialogResult, string text) = await TextInputWindow.Show(this, "Enter broadcast message:", "Server Broadcast");

        if (dialogResult != DialogResult.OK)
            return;

        CommandResult commandResult = CommandDispatcher.SendServerBroadcast(_serverInstance, text);
        HandleCommandResult(commandResult);
    }

    private async void AccountCreateButton_Click(object sender, RoutedEventArgs e)
    {
        const string InputTitle = "Create Account";

        // TODO: Dedicated account creation window
        List<string> inputs = new();

        DialogResult dialogResult = await TextInputWindow.ShowMulti(this, inputs,
            new("Enter email:", InputTitle),
            new("Enter player name:", InputTitle),
            new("Enter password:", InputTitle),
            new("Enter password again:", InputTitle));

        if (dialogResult != DialogResult.OK)
            return;

        string email = inputs[0];
        string playerName = inputs[1];
        string password = inputs[2];
        string passwordAgain = inputs[3];

        if (string.Equals(password, passwordAgain, StringComparison.Ordinal) == false)
        {
            await MessageBoxWindow.Show(this, "Your passwords do not match.", "Error");
            return;
        }

        CommandResult commandResult = CommandDispatcher.SendAccountCreate(_serverInstance, email, playerName, password);
        HandleCommandResult(commandResult);
    }

    private async void AccountChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        const string InputTitle = "Change Password";

        List<string> inputs = new();

        DialogResult dialogResult = await TextInputWindow.ShowMulti(this, inputs,
            new("Enter email:", InputTitle),
            new("Enter password:", InputTitle),
            new("Enter password again:", InputTitle));

        if (dialogResult != DialogResult.OK)
            return;

        string email = inputs[0];
        string password = inputs[1];
        string passwordAgain = inputs[2];

        if (string.Equals(password, passwordAgain, StringComparison.Ordinal) == false)
        {
            await MessageBoxWindow.Show(this, "Your passwords do not match.", "Error");
            return;
        }

        CommandResult commandResult = CommandDispatcher.SendAccountSetPassword(_serverInstance, email, password);
        HandleCommandResult(commandResult);
    }

    private async void AccountChangeUserLevelButton_Click(object sender, RoutedEventArgs e)
    {
        const string InputTitle = "Change User Level";

        List<string> inputs = new();

        DialogResult dialogResult = await TextInputWindow.ShowMulti(this, inputs,
            new("Enter email:", InputTitle),
            new("Enter user level (0-2):", InputTitle, "0"));

        if (dialogResult != DialogResult.OK)
            return;

        string email = inputs[0];
        string userLevelString = inputs[1];

        if (int.TryParse(userLevelString.Trim(), out int userLevel) == false)
        {
            await MessageBoxWindow.Show(this, $"'{userLevelString}' is not an integer number.", "Error");
            return;
        }

        CommandResult commandResult = CommandDispatcher.SendAccountSetUserLevel(_serverInstance, email, userLevel);
        HandleCommandResult(commandResult);
    }

    private async void AccountBanButton_Click(object sender, RoutedEventArgs e)
    {
        (DialogResult dialogResult, string email) = await TextInputWindow.Show(this, "Enter email:", "Account Ban");

        if (dialogResult != DialogResult.OK)
            return;

        CommandResult commandResult = CommandDispatcher.SendAccountBan(_serverInstance, email);
        HandleCommandResult(commandResult);
    }

    private async void AccountUnbanButton_Click(object sender, RoutedEventArgs e)
    {
        (DialogResult dialogResult, string email) = await TextInputWindow.Show(this, "Enter email:", "Account Unban");

        if (dialogResult != DialogResult.OK)
            return;

        CommandResult commandResult = CommandDispatcher.SendAccountUnban(_serverInstance, email);
        HandleCommandResult(commandResult);
    }

    private async void ManualCommandButton_Click(object sender, RoutedEventArgs e)
    {
        (DialogResult dialogResult, string input) = await TextInputWindow.Show(this, "Enter command:", "Manual Command");

        if (dialogResult != DialogResult.OK)
            return;

        CommandResult commandResult = CommandDispatcher.SendCommand(_serverInstance, input);
        HandleCommandResult(commandResult);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    #endregion
}