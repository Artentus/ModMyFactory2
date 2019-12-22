using ReactiveUI;

namespace ModMyFactoryGUI.MVVM
{
    interface IView
    {
        object ViewModel { get; set; }
    }

    interface IView<T> : IView where T : IRoutableViewModel
    {
        new T ViewModel { get; set; }
    }
}
