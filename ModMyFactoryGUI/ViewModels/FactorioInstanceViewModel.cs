using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Game;
using ModMyFactory.WebApi.Factorio;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Tasks.Web;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class FactorioInstanceViewModel : ReactiveObject
    {
        private static readonly Dictionary<string, string> NameTable;

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

        public string Name
        {
            get => GetName(this);
            set => SetName(this, value);
        }

        public AccurateVersion? Version => Instance?.Version;

        static FactorioInstanceViewModel()
        {
            if (!Program.Settings.TryGet(SettingName.FactorioNameTable, out NameTable))
                NameTable = new Dictionary<string, string>();
        }

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

        private static string GetNameKey(ManagedFactorioInstance instance)
        {
            // We use the full path of the instance as unique key
            string result = instance.Directory.FullName.Trim().ToLower();

            // We have to sanitize the path to make sure it's a proper unique key
            result = result.Replace('/', '_');
            result = result.Replace('\\', '_');
            if (result.EndsWith("_")) result = result.Substring(0, result.Length - 1);

            return result;
        }

        private static string GetName(FactorioInstanceViewModel vm)
        {
            var instance = vm.Instance;
            if (instance is null)
            {
                // Doesn't have a name yet (either downloading or extracting)
                return string.Empty;
            }
            else
            {
                string key = GetNameKey(instance);
                return NameTable.GetValue(key, "Factorio");
            }
        }

        private static void SetName(FactorioInstanceViewModel vm, string name)
        {
            var instance = vm.Instance;
            if (instance is null)
            {
                // Can't have a name yet (either downloading or extracting)
                return;
            }
            else
            {
                string key = GetNameKey(instance);
                NameTable[key] = name;
            }
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
