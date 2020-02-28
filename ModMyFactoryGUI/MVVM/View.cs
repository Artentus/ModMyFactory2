using ReactiveUI;

namespace ModMyFactoryGUI.MVVM
{
    static class View
    {
        public static TView CreateAndAttach<TView>(ViewModelBase<TView> viewModel)
            where TView : class, IView, new()
        {
            var view = new TView();
            viewModel.AttachView(view);
            return view;
        }

        public static TViewModel ViewModel<TViewModel>(this IView view)
            where TViewModel : ReactiveObject
        {
            return view.ViewModel as TViewModel;
        }
    }
}
