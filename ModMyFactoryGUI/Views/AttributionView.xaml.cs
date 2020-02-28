using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.ViewModels;

namespace ModMyFactoryGUI.Views
{
    class AttributionView : ReactiveControlBase<AttributionViewModel>
    {
        public AttributionView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
