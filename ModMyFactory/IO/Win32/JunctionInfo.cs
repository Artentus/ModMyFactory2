using System;
using System.IO;

namespace ModMyFactory.IO.Win32
{
    sealed class JunctionInfo : ISymlinkInfo
    {
        string _destinationPath;

        public string Name { get; }

        public string FullName { get; }

        public string DestinationPath
        {
            get => _destinationPath;
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
