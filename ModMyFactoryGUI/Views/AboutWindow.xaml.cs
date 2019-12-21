using Avalonia;
using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.Controls;

namespace ModMyFactoryGUI.Views
{
    partial class AboutWindow : WindowBase
    {
        public AboutWindow()
        {
            this.InitializeComponent();
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
