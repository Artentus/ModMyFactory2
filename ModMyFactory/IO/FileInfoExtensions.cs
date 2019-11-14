using System;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.IO
{
    static class FileInfoExtensions
    {
        public static async Task<FileInfo> CopyToAsync(this FileInfo file, string path)
        {
            var newFile = new FileInfo(path);
            if (!newFile.Directory.Exists) newFile.Directory.Create();

            using (var source = file.OpenRead())
            {
                using (var destination = newFile.Open(FileMode.CreateNew, FileAccess.Write))
                    await source.CopyToAsync(destination);
            }

            return newFile;
        }

        public static async Task<FileInfo> MoveToAsync(this FileInfo file, string path)
        {
            var root1 = Path.GetPathRoot(file.FullName);
            var root2 = Path.GetPathRoot(path);
            if (string.Equals(root1, root2, StringComparison.OrdinalIgnoreCase)) // Moving to same drive is almost instant
            {
                var newFile = new FileInfo(path);
                if (!newFile.Directory.Exists) newFile.Directory.Create();

                file.MoveTo(newFile.FullName);
                return newFile;
            }
            else // Moving to different drive is a copy then delete
            {
                var newFile = await CopyToAsync(file, path);
                file.Delete();
                return newFile;
            }
        }

        public static void Rename(this FileInfo file, string newName) => file.MoveTo(Path.Combine(file.Directory.FullName, newName));

        public static string NameWithoutExtension(this FileInfo file) => Path.GetFileNameWithoutExtension(file.Name);
    }
}
