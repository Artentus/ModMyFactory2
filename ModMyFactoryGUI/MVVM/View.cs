using System;

namespace ModMyFactoryGUI.MVVM
{
    static class View
    {
        public static TView CreateWithViewModel<TView, TViewModel>(out TViewModel viewModel)
            where TView : IView, new()
            where TViewModel : ViewModelBase<TView>
        {
            var view = new TView();
            viewModel = (TViewModel)Activator.CreateInstance(typeof(TViewModel), view);
            view.DataContext = viewModel;
            return view;
        }

        public static TViewModel ViewModel<TViewModel, TView>(this TView view)
            where TView : IView
            where TViewModel : ViewModelBase<TView>
            => view.DataContext as TViewModel;
    }
}
