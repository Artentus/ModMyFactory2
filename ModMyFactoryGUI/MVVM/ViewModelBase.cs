using ReactiveUI;

namespace ModMyFactoryGUI.MVVM
{
    abstract class ViewModelBase<T> : ReactiveObject where T : class, IView
    {
        public T AttachedView { get; private set; }

        protected ViewModelBase()
        { }

        public void AttachView(T view)
        {
            if (!(AttachedView is null))
                AttachedView.ViewModel = null;

            if (!(view is null))
                view.ViewModel = this;
            AttachedView = view;
        }
    }
}
