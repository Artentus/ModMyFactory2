using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ModMyFactoryGUI.Controls.Icons
{
    class VisualThemeIcon : UserControl
    {
        public VisualThemeIcon()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
