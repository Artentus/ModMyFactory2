//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactoryGUI.Tasks.Web
{
    internal class DownloadManager : JobManager<DownloadJob>
    {
        public DownloadManager(IProgress<(DownloadJob, double)> progress)
            : base(progress)
        { }

        public DownloadManager()
            : base()
        { }
    }
}
