using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;

namespace ModMyFactoryGUI.ViewModels
{
    abstract class MainViewModelBase<T> : RoutableViewModelBase<T>, IMainViewModel where T : class, IMainView
    { }
}
