using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.ViewModels;

namespace ModMyFactoryGUI.Views
{
    class OnlineModsView : MainViewBase<OnlineModsViewModel>
    {
        public OnlineModsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
