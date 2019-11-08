using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ModMyFactory.BaseTypes;
using ModMyFactory.IO;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Readers.Zip;

namespace ModMyFactory.Mods
{
    public class ZippedModFile : IModFile
    {
        FileInfo _file;

        public string FilePath => _file.FullName;

        public ModInfo Info { get; }

        public Stream Thumbnail { get; }

        public bool IsExtracted => false;

        public void Delete()
        {
            Thumbnail.Dispose();
            _file.Delete();
        }

        public async Task<IModFile> CopyToAsync(string destination)
        {
            var fullPath = Path.Combine(destination, _file.Name);
            var newFile = await _file.CopyToAsync(fullPath);
            var newThumbnail = await ModFile.CopyThumbnailAsync(Thumbnail);
            return new ZippedModFile(newFile, Info, newThumbnail);
        }

        public async Task MoveToAsync(string destination)
        {
            var fullPath = Path.Combine(destination, _file.Name);
            _file = await _file.MoveToAsync(fullPath);
        }

        /// <summary>
        /// Extracts this mod file.
        /// </summary>
        /// <param name="destination">The path to extract this mod file to, excluding the mod files name itself.</param>
        public async Task<ExtractedModFile> ExtractToAsync(string destination)
        {
            await Task.Run(() =>
            {
                using (var fs = _file.OpenRead())
                {
                    using (var reader = ZipReader.Open(fs))
                    {
                        while (reader.MoveToNextEntry())
                        {
                            if (!reader.Entry.IsDirectory)
                                reader.WriteEntryToDirectory(destination, new ExtractionOptions() { ExtractFullPath = true });
                        }
                    }
                }
            });

            var newDir = new DirectoryInfo(Path.Combine(destination, _file.NameWithoutExtension()));
            var newThumbnail = ModFile.LoadThumbnail(newDir);
            return new ExtractedModFile(newDir, Info, newThumbnail);
        }

        /// <summary>
        /// Extracts this mod file to the same location.
        /// </summary>
        public async Task<ExtractedModFile> ExtractAsync() => await ExtractToAsync(_file.Directory.FullName);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Thumbnail.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ZippedModFile()
        {
            Dispose(false);
        }

        internal ZippedModFile(FileInfo file, ModInfo info, Stream thumbnail)
        {
            _file = file;
            Info = info;
            Thumbnail = thumbnail;
        }


        /// <summary>
        /// Tries to load a mod file.
        /// </summary>
        /// <param name="file">The archive file to load.</param>
        public static async Task<(bool, ZippedModFile)> TryLoadAsync(FileInfo file)
        {
            if (!file.Exists) return (false, null);

            bool hasInfo = false;
            ModInfo info = default;
            Stream thumbnail = null;
            try
            {
                using (var fs = file.OpenRead())
                {
                    using (var reader = ZipReader.Open(fs))
                    {
                        while (reader.MoveToNextEntry())
                        {
                            var entry = reader.Entry;
                            if (!entry.IsDirectory)
                            {
                                if (entry.Key.StartsWith(file.NameWithoutExtension() + "/", StringComparison.InvariantCultureIgnoreCase)
                                    && (entry.Key.IndexOf('/') == entry.Key.LastIndexOf('/'))) // All top level files
                                {
                                    if (entry.Key.EndsWith("info.json"))
                                    {
                                        var stream = new MemoryStream();
                                        await Task.Run(() => reader.WriteEntryTo(stream));

                                        string json = null;
                                        using (var sr = new StreamReader(stream, Encoding.UTF8))
                                            json = await sr.ReadToEndAsync();

                                        info = ModInfo.FromJson(json);
                                        hasInfo = true;
                                    }
                                    else if (entry.Key.EndsWith("thumbnail.png"))
                                    {
                                        thumbnail = new MemoryStream();
                                        await Task.Run(() => reader.WriteEntryTo(thumbnail));
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch
            {
                thumbnail?.Dispose(); // In case of exception dispose thumbnail stream
                throw;
            }

            if (!ModFile.TryParseFileName(file.NameWithoutExtension(), out var fileName, out var fileVersion)
                || (fileName != info.Name) || (fileVersion != info.Version))
            {
                thumbnail?.Dispose();
                return (false, null);
            }

            return (hasInfo, hasInfo ? new ZippedModFile(file, info, thumbnail) : null);
        }

        /// <summary>
        /// Tries to load a mod file.
        /// </summary>
        /// <param name="path">The path to an archive file to load.</param>
        public static async Task<(bool, ZippedModFile)> TryLoadAsync(string path)
        {
            var file = new FileInfo(path);
            return await TryLoadAsync(file);
        }

        /// <summary>
        /// Loads a mod file.
        /// </summary>
        /// <param name="file">The archive file to load.</param>
        public static async Task<ZippedModFile> LoadAsync(FileInfo file)
        {
            (bool success, var result) = await TryLoadAsync(file);
            if (!success) throw new InvalidPathException("The path does not point to a valid mod file.");
            return result;
        }

        /// <summary>
        /// Loads a mod file.
        /// </summary>
        /// <param name="path">The path to an archive file to load.</param>
        public static async Task<ZippedModFile> LoadAsync(string path)
        {
            var file = new FileInfo(path);
            return await LoadAsync(file);
        }
    }
}
