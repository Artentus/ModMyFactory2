//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Media.Imaging;
using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.WebApi;
using ModMyFactory.WebApi.Mods;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Tasks.Web;
using ReactiveUI;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class OnlineModViewModel : ReactiveObject, IDisposable
    {
        private static readonly CollectionView<ModReleaseViewModel> _emptyReleases = new CollectionView<ModReleaseViewModel>(new ModReleaseViewModel[0]);

        private readonly Manager _manager;
        private readonly DownloadQueue _downloadQueue;
        private ApiModInfo _info;
        private volatile bool _isExtended;
        private ModReleaseViewModel[] _releases;
        private ModReleaseViewModel _selectedRelease;

        private bool isDisposed = false;

        public ApiModInfo Info
        {
            get => _info;
            private set
            {
                this.RaiseAndSetIfChanged(ref _info, value, nameof(Info));

                this.RaisePropertyChanged(nameof(DisplayName));
                this.RaisePropertyChanged(nameof(Author));
                this.RaisePropertyChanged(nameof(DownloadCount));
                this.RaisePropertyChanged(nameof(Summary));
                this.RaisePropertyChanged(nameof(Description));
                this.RaisePropertyChanged(nameof(FactorioVersion));
                this.RaisePropertyChanged(nameof(HasChangelog));
                this.RaisePropertyChanged(nameof(Changelog));
                this.RaisePropertyChanged(nameof(HasFaq));
                this.RaisePropertyChanged(nameof(Faq));
            }
        }

        public bool HasThumbnail { get; private set; }
        public IBitmap Thumbnail { get; private set; }

        public bool HasHomepage { get; private set; }
        public Uri Homepage { get; private set; }

        public bool HasGitHub { get; private set; }
        public Uri GitHubUrl { get; private set; }

        public bool HasLicense { get; private set; }
        public string LicenseName { get; private set; }
        public bool HasLicenseUrl { get; private set; }
        public Uri LicenseUrl { get; private set; }

        public bool HasChangelog => !string.IsNullOrWhiteSpace(Changelog);
        public bool HasFaq => !string.IsNullOrWhiteSpace(Faq);

        public CollectionView<ModReleaseViewModel> Releases { get; private set; }

        public ModReleaseViewModel SelectedRelease
        {
            get => _selectedRelease;
            set => this.RaiseAndSetIfChanged(ref _selectedRelease, value, nameof(SelectedRelease));
        }


        // Store information for fuzzy search
        public bool MatchesSearch { get; private set; } = true;

        public int SearchScore { get; private set; } = 0;


        public AccurateVersion? FactorioVersion
        {
            get
            {
                if (Info.TryGetLatestRelease(out var release))
                    return release.Info.FactorioVersion.ToMajor();

                return null;
            }
        }

        public string Changelog => Info.Changelog;

        public string Faq => Info.Faq;

        public string DisplayName => Info.DisplayName;

        public string Author => Info.Author;

        public int DownloadCount => Info.DownloadCount;

        public string Summary => Info.Summary;

        public string Description => Info.Description;

        public OnlineModViewModel(ApiModInfo info, Manager manager, DownloadQueue downloadQueue)
            => (_info, _manager, _downloadQueue, Releases) = (info, manager, downloadQueue, _emptyReleases);

        ~OnlineModViewModel()
        {
            Dispose(false);
        }

        private void LoadHomepage()
        {
            HasHomepage = Uri.TryCreate(Info.Homepage, UriKind.Absolute, out var homepage);
            this.RaisePropertyChanged(nameof(HasHomepage));
            if (HasHomepage)
            {
                Homepage = homepage;
                this.RaisePropertyChanged(nameof(Homepage));
            }
        }

        private void LoadGitHub()
        {
            HasGitHub = Uri.TryCreate(Info.GitHubUrl, UriKind.Absolute, out var gitHubUrl);
            this.RaisePropertyChanged(nameof(HasGitHub));
            if (HasGitHub)
            {
                GitHubUrl = gitHubUrl;
                this.RaisePropertyChanged(nameof(GitHubUrl));
            }
        }

        private void LoadLicense()
        {
            LicenseName = Info.License.Name;
            this.RaisePropertyChanged(nameof(LicenseName));
            HasLicense = !string.IsNullOrWhiteSpace(LicenseName);
            this.RaisePropertyChanged(nameof(HasLicense));
            if (HasLicense)
            {
                HasLicenseUrl = Uri.TryCreate(Info.License.Url, UriKind.Absolute, out var licenseUrl);
                this.RaisePropertyChanged(nameof(HasLicenseUrl));
                if (HasLicenseUrl)
                {
                    LicenseUrl = licenseUrl;
                    this.RaisePropertyChanged(nameof(LicenseUrl));
                }
            }
        }

        private void LoadReleases()
        {
            if (Info.Releases is null)
            {
                _releases = null;
                Releases = _emptyReleases;
            }
            else
            {
                _releases = new ModReleaseViewModel[Info.Releases.Length];
                for (int i = 0; i < _releases.Length; i++)
                    _releases[i] = new ModReleaseViewModel(Info.Releases[i], Info.Name, Info.DisplayName, _manager, _downloadQueue);
                Releases = new CollectionView<ModReleaseViewModel>(_releases, (a, b) => b.Version.CompareTo(a.Version));
            }

            this.RaisePropertyChanged(nameof(Releases));
        }

        private async Task LoadThumbnail()
        {
            HasThumbnail = !string.IsNullOrWhiteSpace(Info.ThumbnailUrl);
            this.RaisePropertyChanged(nameof(HasThumbnail));
            if (HasThumbnail)
            {
                if (Uri.TryCreate(Info.ThumbnailUrl, UriKind.Absolute, out Uri url))
                {
                    using var wc = new WebClient();
                    try
                    {
                        byte[] data = await wc.DownloadDataTaskAsync(url);
                        using var stream = new MemoryStream(data);
                        Thumbnail = new Bitmap(stream);
                        this.RaisePropertyChanged(nameof(Thumbnail));
                    }
                    catch
                    {
                        // This is only loading the thumbnail, so it's ok to just agressively catch all exceptions.
                    }
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (!(_releases is null))
                {
                    foreach (var release in _releases)
                        release.Dispose();
                }

                Thumbnail?.Dispose();
                isDisposed = true;
            }
        }

        public async Task LoadExtendedInfoAsync()
        {
            if (!_isExtended)
            {
                try
                {
                    _isExtended = true;
                    Info = await ModApi.RequestModInfoAsync(Info.Name);

                    LoadHomepage();
                    LoadGitHub();
                    LoadLicense();
                    LoadReleases();
                    await LoadThumbnail();
                }
                catch (ApiException ex)
                {
                    _isExtended = false;
                    await MessageHelper.ShowMessageForApiException(ex);
                }
            }
        }

        public void ApplyFuzzyFilter(in string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                MatchesSearch = true;
                SearchScore = 0;
                return;
            }

            // We allow searching for title, name and author
            bool titleMatches = DisplayName.FuzzyMatch(filter, out int titleScore);
            bool nameMatches = Info.Name.FuzzyMatch(filter, out int nameScore);
            bool authorMatches = Author.FuzzyMatch(filter, out int authorScore);

            int score = 0;
            if (titleMatches) score += titleScore;
            if (nameMatches) score += nameScore;
            if (authorMatches) score += authorScore;

            MatchesSearch = titleMatches || nameMatches || authorMatches;
            SearchScore = score;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
