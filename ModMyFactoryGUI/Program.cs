using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

namespace ModMyFactoryGUI
{
    static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static int Main(string[] args)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);

        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
#if DEBUG
                .LogToDebug()
#endif
                .UsePlatformDetect()
                .UseReactiveUI()
                .UseManagedSystemDialogs();
        }
            
    }
}
