using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.ViewModels;
using ModMyFactoryGUI.Views;

namespace ModMyFactoryGUI
{
    partial class App : Application
    {
        public static new App Current => Application.Current as App;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = View.CreateWithViewModel<MainWindow, MainWindowViewModel>(out _);

            base.OnFrameworkInitializationCompleted();
        }
    }
}
