using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ModMyFactoryGUI.Views
{
    public class OnlineModView : UserControl
    {
        public OnlineModView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
