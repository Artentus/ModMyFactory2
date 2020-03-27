using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Game;
using ModMyFactory.WebApi.Factorio;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Tasks.Web;
using ReactiveUI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class FactorioInstanceViewModel : ReactiveObject
    {
        private readonly Manager _manager;
        private readonly LocationManager _locations;
        private bool _isInDownloadQueue, _isDownloading, _isExtracting, _isInstalled;
        private ManagedFactorioInstance _instance;

        public bool IsInDownloadQueue
        {
            get => _isInDownloadQueue;
            private set => this.RaiseAndSetIfChanged(ref _isInDownloadQueue, value, nameof(IsInDownloadQueue));
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            private set => this.RaiseAndSetIfChanged(ref _isDownloading, value, nameof(IsDownloading));
        }

        public bool IsExtracting
        {
            get => _isExtracting;
            private set => this.RaiseAndSetIfChanged(ref _isExtracting, value, nameof(IsExtracting));
        }

        public bool IsInstalled
        {
            get => _isInstalled;
            private set => this.RaiseAndSetIfChanged(ref _isInstalled, value, nameof(IsInstalled));
        }

        public ManagedFactorioInstance Instance
        {
            get => _instance;
            private set
            {
                if (value != _instance)
                {
                    _instance = value;
                    this.RaisePropertyChanged(nameof(Instance));

                    this.RaisePropertyChanged(nameof(Name));
                    this.RaisePropertyChanged(nameof(Version));
                }
            }
        }

        public string Name => "";

        public AccurateVersion? Version => Instance?.Version;

        /// <summary>
        /// Use this constructor for already existing instances
        /// </summary>
        public FactorioInstanceViewModel(Manager manager, ManagedFactorioInstance instance)
        {
            if (manager is null) throw new ArgumentNullException(nameof(manager));
            _manager = manager;

            if (instance is null) throw new ArgumentNullException(nameof(instance));
            _instance = instance;

            _isInstalled = true;
        }

        /// <summary>
        /// If this constructor is used the instance has to be created asyncroneously later
        /// </summary>
        public FactorioInstanceViewModel(Manager manager, LocationManager locations, bool download)
        {
            if (manager is null) throw new ArgumentNullException(nameof(manager));
            _manager = manager;

            if (locations is null) throw new ArgumentNullException(nameof(locations));
            _locations = locations;

            _isInDownloadQueue = download;
            _isExtracting = !download;
        }

        public async Task<bool> TryCreateDownloadAsync(DownloadQueue downloadQueue, bool experimental)
        {
            if (downloadQueue is null) throw new ArgumentNullException(nameof(downloadQueue));

            // We can only download the latest available version (stable or experimental)
            // since that is all the API gives us. If other versions are desired the user
            // has to download them manually and use the extract option instead.
            var (stable, exp) = await DownloadApi.GetReleasesAsync();
            var toDownload = experimental ? exp : stable;

            if (toDownload.TryGetValue(FactorioBuild.Alpha, out var version))
            {
                var (success, username, token) = await App.Current.Credentials.TryLogInAsync();
                if (success.IsTrue())
                {
                    IsInDownloadQueue = true;

                    // Create a download job
                    var job = new DownloadFactorioJob(username, token,
                        version, FactorioBuild.Alpha, PlatformHelper.GetCurrentPlatform());

                    // Use an actual function instead of a lambda
                    // because we need to unsubscribe it again
                    void OnProgressChanged(object sender, double progress)
                    {
                        IsInDownloadQueue = false;
                        IsDownloading = true;
                        job.Progress.ProgressChanged -= OnProgressChanged;
                    }

                    // Start download and wait for it to complete
                    job.Progress.ProgressChanged += OnProgressChanged;
                    await downloadQueue.AddJobAsync(job);

                    IsInDownloadQueue = false;
                    IsDownloading = false;

                    // After download we extract
                    if (job.Success) return await TryCreateExtractAsync(job.File);
                    else return false;
                }
            }

            return false;
        }

        public async Task<bool> TryCreateExtractAsync(FileInfo file)
        {
            IsExtracting = true;
            string dirName = _locations.GenerateNewFactorioDirectoryName();
            var (success, instance) = await Factorio.TryExtract(file, _locations.GetFactorioDir().FullName, dirName);
            IsExtracting = false;

            if (success)
            {
                IsInstalled = true;
                Instance = _manager.AddInstance(instance);
                return true;
            }
            else
            {
                // ToDo: show error message
                return false;
            }
        }
    }
}
