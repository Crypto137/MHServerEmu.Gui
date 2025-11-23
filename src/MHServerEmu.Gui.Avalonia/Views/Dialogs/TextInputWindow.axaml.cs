using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MHServerEmu.Gui.Avalonia.Views.Dialogs;

public readonly struct TextInputSettings
{
    public string Title { get; init; }
    public string Label { get; init; }
    public string DefaultInput { get; init; }

    public TextInputSettings()
    {
        Title = "";
        Label = "";
        DefaultInput = "";
    }

    public TextInputSettings(string label, string title = "", string defaultInput = "")
    {
        Title = title;
        Label = label;
        DefaultInput = defaultInput;
    }
}

public partial class TextInputWindow : Window
{
    public DialogResult Result { get; private set; } = DialogResult.None;
    public string Input { get; private set; } = "";

    public TextInputWindow()
    {
        InitializeComponent();

        InputTextBox.Text = "";     // clear designer placeholder
    }

    public TextInputWindow(TextInputSettings settings) : this()
    {
        if (string.IsNullOrWhiteSpace(settings.Title) == false)
            Title = settings.Title;

        if (string.IsNullOrWhiteSpace(settings.Label) == false)
            LabelTextBlock.Text = settings.Label;

        if (string.IsNullOrWhiteSpace(settings.DefaultInput) == false)
            InputTextBox.Text = settings.DefaultInput;
    }

    public static async Task<(DialogResult, string)> Show(Window owner, string label = "", string title = "", string defaultInput = "")
    {
        TextInputSettings settings = new()
        {
            Title = title,
            Label = label,
            DefaultInput = defaultInput,
        };

        TextInputWindow textInputWindow = new(settings);
        await textInputWindow.ShowDialog(owner);

        return (textInputWindow.Result, textInputWindow.Input);
    }

    public static async Task<DialogResult> ShowMulti(Window owner, List<string> inputs, params TextInputSettings[] inputSettings)
    {
        foreach (TextInputSettings settings in inputSettings)
        {
            TextInputWindow textInputWindow = new(settings);
            await textInputWindow.ShowDialog(owner);

            DialogResult dialogResult = textInputWindow.Result;
            string input = textInputWindow.Input;

            if (dialogResult != DialogResult.OK)
                return dialogResult;

            inputs.Add(input);
        }

        return DialogResult.OK;
    }

    private void AcceptInput(DialogResult result)
    {
        // TODO: Additional validation for things like booleans?

        Result = result;
        Input = InputTextBox.Text;

        Close();
    }

    #region Event Handlers

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        AcceptInput(DialogResult.OK);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        AcceptInput(DialogResult.Cancel);
    }

    private void Window_Activated(object sender, System.EventArgs e)
    {
        InputTextBox.SelectionStart = 0;
        InputTextBox.SelectionEnd = InputTextBox.Text.Length;
        InputTextBox.Focus();
    }

    #endregion
}