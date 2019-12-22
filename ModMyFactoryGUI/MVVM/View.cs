using ReactiveUI;
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
            view.ViewModel = viewModel;
            return view;
        }

        public static TView CreateWithViewModel<TView, TViewModel>(out TViewModel viewModel, IScreen hostScreen)
            where TView : IView, new()
            where TViewModel : RoutableViewModelBase<TView>
        {
            var view = new TView();
            viewModel = (TViewModel)Activator.CreateInstance(typeof(TViewModel), view, hostScreen);
            view.ViewModel = viewModel;
            return view;
        }

        public static TViewModel ViewModel<TViewModel, TView>(this TView view)
            where TView : IView
            where TViewModel : ViewModelBase<TView>
            => view.ViewModel as TViewModel;
    }
}
