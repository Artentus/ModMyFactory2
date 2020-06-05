//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Export;
using ModMyFactory.Mods;
using ReactiveUI;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModExportViewModel : ReactiveObject
    {
        private bool _useLatestVersion, _useFactorioVersion, _useSpecificVersion;
        private bool _includeFile, _downloadNewer;

        public Mod Mod { get; }

        public bool UseLatestVersion
        {
            get => _useLatestVersion;
            set => this.RaiseAndSetIfChanged(ref _useLatestVersion, value, nameof(UseLatestVersion));
        }

        public bool UseFactorioVersion
        {
            get => _useFactorioVersion;
            set => this.RaiseAndSetIfChanged(ref _useFactorioVersion, value, nameof(UseFactorioVersion));
        }

        public bool UseSpecificVersion
        {
            get => _useSpecificVersion;
            set => this.RaiseAndSetIfChanged(ref _useSpecificVersion, value, nameof(UseSpecificVersion));
        }

        public bool IncludeFile
        {
            get => _includeFile;
            set => this.RaiseAndSetIfChanged(ref _includeFile, value, nameof(IncludeFile));
        }

        public bool DownloadNewer
        {
            get => _downloadNewer;
            set => this.RaiseAndSetIfChanged(ref _downloadNewer, value, nameof(DownloadNewer));
        }

        public string DisplayName => Mod.DisplayName;

        public ModExportViewModel(Mod mod)
        {
            _useLatestVersion = true;
            Mod = mod;
        }

        public ExportMode GetExportMode()
        {
#pragma warning disable CS0612
            var mode
                = UseLatestVersion ? ExportMode.LatestVersion
                : UseFactorioVersion ? ExportMode.FactorioVersion
                : UseSpecificVersion ? ExportMode.SpecificVersion
                : ExportMode.Invalid;
#pragma warning restore CS0612

            if (IncludeFile) mode |= ExportMode.Included;
            if (DownloadNewer) mode |= ExportMode.DownloadNewer;

            return mode;
        }
    }
}
