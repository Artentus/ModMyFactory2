using ModMyFactoryGUI.Views;
using System.Collections.Generic;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class FactorioViewModel : MainViewModelBase<FactorioView>
    {
        protected override List<IMenuItemViewModel> GetEditMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }

        protected override List<IMenuItemViewModel> GetFileMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }
    }
}
