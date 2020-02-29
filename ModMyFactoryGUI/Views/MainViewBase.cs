using Avalonia.Controls;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.ViewModels;

namespace ModMyFactoryGUI.Views
{
    interface IMainView : IView<IMainViewModel>, IControl
    { }

    abstract class MainViewBase<T> : ReactiveControlBase<T>, IMainView where T : class, IMainViewModel
    {
        IMainViewModel IView<IMainViewModel>.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (T)value;
        }
    }
}
