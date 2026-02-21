using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace StudyHub;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // головне вікно UI з підключеним сервісом.
            desktop.MainWindow = new MainWindow(new StudyHubService(new StudyHubStorage()));
        }

        base.OnFrameworkInitializationCompleted();
    }
}
