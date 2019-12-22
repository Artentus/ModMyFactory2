using Avalonia.ReactiveUI;
using ModMyFactoryGUI.MVVM;
using ReactiveUI;

namespace ModMyFactoryGUI.Controls
{
    abstract class ReactiveControlBase<T> : ReactiveUserControl<T>, IView<T> where T : class, IRoutableViewModel
    {
        object IView.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (T)value;
        }
    }
}
