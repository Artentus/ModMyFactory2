//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal static class FileHelper
    {
        public static
#if NETFULL
            async
#endif
            Task<string> ReadAllTextAsync(string path, Encoding encoding)
        {
#if NETFULL
            using var stream = File.OpenRead(path);
            using var reader = new StreamReader(stream, encoding);
            return await reader.ReadToEndAsync();
#elif NETCORE
            return File.ReadAllTextAsync(path, encoding);
#endif
        }

        public static
#if NETFULL
            async
#endif
            Task WriteAllTextAsync(string path, string contents, Encoding encoding)
        {
#if NETFULL
            using var stream = File.OpenWrite(path);
            using var writer = new StreamWriter(stream, encoding);
            await writer.WriteAsync(contents);
#elif NETCORE
            return File.WriteAllTextAsync(path, contents, encoding);
#endif
        }

        public static bool IsOnSameVolume(string path1, string path2)
        {
            string root1 = Path.GetPathRoot(path1);
            string root2 = Path.GetPathRoot(path2);

            var info1 = new DriveInfo(root1);
            var info2 = new DriveInfo(root2);

            return string.Equals(info1.Name, info2.Name, StringComparison.OrdinalIgnoreCase);
        }

        public static async ValueTask<DirectoryInfo> MoveDirectoryWithStatusAsync(DirectoryInfo directory, string destination, bool overwrite = false)
        {
            var mainWindow = App.Current?.MainWindow;
            if (!(mainWindow is null))
            {
                // UI is loaded, display status window
            }
            else
            {
                // No UI, proceed silently
                return await directory.MoveToAsync(destination, overwrite);
            }
        }
    }
}
