using ReactiveUI;

namespace ModMyFactoryGUI.MVVM
{
    abstract class ScreenBase<T> : ViewModelBase<T>, IScreen where T : IView
    {
        public RoutingState Router { get; }

        protected ScreenBase(T view)
            : base(view)
        {
            Router = new RoutingState();
        }
    }
}
