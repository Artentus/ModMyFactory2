using Avalonia;
using Avalonia.Markup.Xaml;

namespace ModMyFactory.Gui.Views
{
    class MainWindow : WindowBase
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
