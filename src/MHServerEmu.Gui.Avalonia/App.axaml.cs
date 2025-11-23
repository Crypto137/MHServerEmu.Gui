using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MHServerEmu.Gui.Avalonia.Views;
using MHServerEmu.ServerManagement;

namespace MHServerEmu.Gui.Avalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                ServerManager serverManager = new();
                desktop.MainWindow = new MainWindow(serverManager);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}