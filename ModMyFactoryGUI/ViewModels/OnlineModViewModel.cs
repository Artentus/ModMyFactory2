using ModMyFactory.WebApi.Mods;
using ReactiveUI;

namespace ModMyFactoryGUI.ViewModels
{
    internal class OnlineModViewModel : ReactiveObject
    {
        public ApiModInfo Info { get; }

        public string DisplayName => Info.DisplayName;

        public string Author => Info.Author;

        public int DownloadCount => Info.DownloadCount;

        public string Summary => Info.Summary;

        public OnlineModViewModel(ApiModInfo info)
            => Info = info;
    }
}
