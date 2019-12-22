using ReactiveUI;

namespace ModMyFactoryGUI.MVVM
{
    abstract class ViewModelBase<T> : ReactiveObject where T : IView
    {
        public T AttachedView { get; }

        protected ViewModelBase(T view)
        {
            AttachedView = view;
        }
    }
}
