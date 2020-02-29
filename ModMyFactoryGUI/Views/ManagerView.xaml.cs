using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.ViewModels;

namespace ModMyFactoryGUI.Views
{
    class ManagerView : MainViewBase<ManagerViewModel>
    {
        public ManagerView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
