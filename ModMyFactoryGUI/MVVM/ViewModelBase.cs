using ReactiveUI;

namespace ModMyFactoryGUI.MVVM
{
    abstract class ViewModelBase<T> : ReactiveObject where T : IView
    {
        public T View { get; }

        protected ViewModelBase(T view)
        {
            View = view;
        }
    }
}
