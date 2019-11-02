using Avalonia;
using Avalonia.Markup.Xaml;

namespace ModMyFactory.Gui
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
