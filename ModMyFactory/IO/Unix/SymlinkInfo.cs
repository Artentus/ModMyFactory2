//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Mono.Unix;
using System.IO;

namespace ModMyFactory.IO.Unix
{
    internal sealed class SymlinkInfo : ISymlinkInfo
    {
        private readonly UnixSymbolicLinkInfo link;

        public string DestinationPath
        {
            get => link.ContentsPath;
            set => link.CreateSymbolicLinkTo(value);
        }

        public string Name => link.Name;

        public string FullName => link.FullName;
        public bool Exists => link.Exists && link.IsSymbolicLink && link.HasContents;

        public SymlinkInfo(string path)
        {
            link = new UnixSymbolicLinkInfo(path);
            if (link.Exists && !link.IsSymbolicLink)
                throw new IOException("File is not a symbolic link.");
        }

        public void Create(string destination) => link.CreateSymbolicLinkTo(destination);

        public void Delete() => link.Delete();
    }
}
