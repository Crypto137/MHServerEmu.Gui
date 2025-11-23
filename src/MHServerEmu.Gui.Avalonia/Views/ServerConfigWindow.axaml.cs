using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MHServerEmu.Gui.Avalonia.Models;
using MHServerEmu.Gui.Avalonia.Views.Dialogs;
using MHServerEmu.ServerManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MHServerEmu.Gui.Avalonia.Views;

public partial class ServerConfigWindow : Window
{
    private readonly ServerSettings _serverSettings;

    private readonly Dictionary<string, List<ServerConfigSetting>> _settingsBySection = new();

    public ServerConfigWindow()
    {
        InitializeComponent();
    }

    public ServerConfigWindow(ServerSettings serverSettings) : this()
    {
        _serverSettings = serverSettings;
        InitializeConfigListBoxes();
    }

    private void InitializeConfigListBoxes()
    {
        if (_serverSettings == null)
            return;

        ConfigSectionListBox.Items.Clear();

        Dictionary<string, Dictionary<string, string>> baseData = new();
        _serverSettings.Config.GetData(baseData);

        Dictionary<string, Dictionary<string, string>> overrideData = new();
        _serverSettings.ConfigOverride.GetData(overrideData);

        foreach (var section in baseData)
        {
            string sectionName = section.Key;

            overrideData.TryGetValue(sectionName, out Dictionary<string, string> sectionOverrides);

            // Define a tag separately in case we ever introduce more user-friendly names for categories.
            ListBoxItem sectionNode = new() { Content = sectionName, Tag = sectionName };
            ConfigSectionListBox.Items.Add(sectionNode);

            List<ServerConfigSetting> settingList = new();
            _settingsBySection[sectionName] = settingList;

            foreach (var kvp in section.Value)
            {
                string settingName = kvp.Key;
                string settingValue = kvp.Value;

                string settingValueOverride = null;
                sectionOverrides?.TryGetValue(kvp.Key, out settingValueOverride);

                ServerConfigSetting setting = new()
                {
                    Section = sectionName,
                    Name = settingName,
                    Value = settingValue,
                    ValueOverride = settingValueOverride,
                };

                settingList.Add(setting);
            }
        }

        SelectSection(baseData.First().Key);
    }

    private void SelectSection(string sectionName)
    {
        // ConfigSettingsListBox can be null during initialization
        ItemCollection settingItems = ConfigSettingsListBox?.Items;
        if (settingItems == null)
            return;

        settingItems.Clear();

        if (_settingsBySection.TryGetValue(sectionName, out List<ServerConfigSetting> sectionSettings) == false)
            return;

        foreach (ServerConfigSetting setting in sectionSettings)
        {
            ListBoxItem settingItem = new()
            {
                Content = $"{setting.Name}: {setting.ValueOverride ?? setting.Value}{(setting.IsDirty ? $"*" : "")}",
                FontWeight = setting.ValueOverride != null ? FontWeight.Bold : FontWeight.Normal,
                Tag = setting,
            };

            settingItems.Add(settingItem);
        }
    }

    private async void EditSetting(ServerConfigSetting setting)
    {
        (DialogResult result, string input) = await TextInputWindow.Show(this, setting.Name, "Edit Setting", setting.ValueOverride ?? setting.Value);

        if (result != DialogResult.OK)
            return;

        // Remove the override if we got empty input or it matches the default value.
        if (string.IsNullOrWhiteSpace(input) || input.Equals(setting.Value, StringComparison.OrdinalIgnoreCase))
            input = null;

        // Early out if the override is the new override matches the old one.
        if (string.Equals(input, setting.ValueOverride, StringComparison.OrdinalIgnoreCase))
            return;

        // Update override and reselect the section to update
        setting.ValueOverride = input;
        setting.IsDirty = true;
        SelectSection(setting.Section);

        // Restore setting selection
        foreach (object obj in ConfigSettingsListBox.Items)
        {
            if (obj is not ListBoxItem item)
                continue;

            if (item.Tag is not ServerConfigSetting tagSetting)
                continue;

            if (tagSetting != setting)
                continue;

            item.IsSelected = true;
            break;
        }
    }

    private void SaveSettings()
    {
        bool isDirty = false;

        foreach (List<ServerConfigSetting> settings in _settingsBySection.Values)
        {
            foreach (ServerConfigSetting setting in settings)
            {
                if (setting.IsDirty == false)
                    continue;

                _serverSettings.ConfigOverride.SetValue(setting.Section, setting.Name, setting.ValueOverride);

                isDirty |= true;
                setting.IsDirty = false;
            }
        }

        if (isDirty)
            _serverSettings.ConfigOverride.Save();
    }

    #region Event Handlers

    private void ConfigSectionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ListBoxItem selectedItem = e.AddedItems.Count > 0 ? e.AddedItems[0] as ListBoxItem : null;
        if (selectedItem == null)
            return;

        if (selectedItem.Tag is not string sectionName)
            return;

        SelectSection(sectionName);
    }

    private void ConfigSettingsListBox_DoubleTapped(object sender, TappedEventArgs e)
    {
        if (ConfigSettingsListBox.SelectedItem is not ListBoxItem selectedSettingItem)
            return;

        if (selectedSettingItem.Tag is not ServerConfigSetting setting)
            return;

        EditSetting(setting);
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    #endregion
}