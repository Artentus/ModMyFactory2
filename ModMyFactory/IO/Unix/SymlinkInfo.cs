using System.IO;
using Mono.Unix;

namespace ModMyFactory.IO.Unix
{
    sealed class SymlinkInfo : ISymlinkInfo
    {
        readonly UnixSymbolicLinkInfo link;

        public string Name => link.Name;

        public string FullName => link.FullName;

        public string DestinationPath
        {
            get => link.ContentsPath;
            set => link.CreateSymbolicLinkTo(value);
        }

        public bool Exists => link.Exists && link.IsSymbolicLink && link.HasContents;

        public SymlinkInfo(string path)
        {
            link = new UnixSymbolicLinkInfo(path);
            if (link.Exists && !link.IsSymbolicLink)
                throw new IOException("File is not a symbolic link.");
        }

        public void Delete() => link.Delete();
    }
}
