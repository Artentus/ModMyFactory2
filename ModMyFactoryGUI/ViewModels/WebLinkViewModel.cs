using ReactiveUI;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class WebLinkViewModel : ReactiveObject
    {
        public string LinkText { get; }

        public string Url { get; }

        public WebLinkViewModel(string linkText, string url)
            => (LinkText, Url) = (linkText, url);
    }
}
