//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.IO;

namespace ModMyFactory.IO.Win32
{
    internal sealed class JunctionInfo : ISymlinkInfo
    {
        private string _destinationPath;

        public string Name { get; }

        public string FullName { get; }

        public string DestinationPath
        {
            get => _destinationPath ?? string.Empty;
            set
            {
                _destinationPath = value;
                Junction.SetDestination(FullName, value);
            }
        }

        public bool Exists => Junction.Exists(FullName);

        public JunctionInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            FullName = Path.GetFullPath(path);
            Name = Path.GetFileName(FullName.TrimEnd(Path.DirectorySeparatorChar));

            if (Junction.Exists(FullName))
                _destinationPath = Junction.GetDestination(FullName);
        }

        public void Create(string destination)
        {
            _destinationPath = destination;
            Junction.Create(FullName, destination);
        }

        public void Delete() => Junction.Delete(FullName);
    }
}
