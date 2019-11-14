using System;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.IO
{
    static class DirectoryInfoExtensions
    {
        public static async Task<DirectoryInfo> CopyToAsync(this DirectoryInfo directory, string path)
        {
            var newDir = new DirectoryInfo(path);
            if (!newDir.Exists) newDir.Create();

            foreach (var file in directory.EnumerateFiles())
                await file.CopyToAsync(Path.Combine(newDir.FullName, file.Name));

            foreach (var subDir in directory.EnumerateDirectories())
                await CopyToAsync(subDir, Path.Combine(newDir.FullName, subDir.Name));

            return newDir;
        }

        public static async Task<DirectoryInfo> MoveToAsync(this DirectoryInfo directory, string path)
        {
            var root1 = Path.GetPathRoot(directory.FullName);
            var root2 = Path.GetPathRoot(path);
            if (string.Equals(root1, root2, StringComparison.OrdinalIgnoreCase)) // Functionality integrated
            {
                directory.MoveTo(path);
                return directory;
            }
            else // Moving all files one-by-one
            {
                var newDir = new DirectoryInfo(path);
                if (!newDir.Exists) newDir.Create();

                foreach (var file in directory.EnumerateFiles())
                    await file.MoveToAsync(Path.Combine(newDir.FullName, file.Name));

                foreach (var subDir in directory.EnumerateDirectories())
                    await MoveToAsync(subDir, Path.Combine(newDir.FullName, subDir.Name));

                directory.Delete();
                return newDir;
            }
        }

        public static void Rename(this DirectoryInfo directory, string newName) => directory.MoveTo(Path.Combine(directory.Parent.FullName, newName));
    }
}
