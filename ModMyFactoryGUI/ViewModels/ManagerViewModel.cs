using ModMyFactoryGUI.Views;
using System.Collections.Generic;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class ManagerViewModel : MainViewModelBase<ManagerView>
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
