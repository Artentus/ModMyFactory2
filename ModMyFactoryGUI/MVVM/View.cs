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
    }
}
