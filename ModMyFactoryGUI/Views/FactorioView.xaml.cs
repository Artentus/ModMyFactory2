using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.ViewModels;

namespace ModMyFactoryGUI.Views
{
    class FactorioView : MainViewBase<FactorioViewModel>
    {
        public FactorioView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
