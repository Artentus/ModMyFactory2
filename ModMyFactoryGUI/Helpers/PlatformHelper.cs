using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ModMyFactoryGUI.Helpers
{
    static class PlatformHelper
    {
        public static void OpenWebUrl(string url)
        {
#if NETFULL
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
#elif NETCORE
            // Code according to Microsoft
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}
