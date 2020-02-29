using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.ViewModels;

namespace ModMyFactoryGUI.Views
{
    class SettingsView : MainViewBase<SettingsViewModel>
    {
        public SettingsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
