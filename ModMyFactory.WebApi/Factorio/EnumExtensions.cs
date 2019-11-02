using System.Collections.Generic;

namespace ModMyFactory.WebApi.Factorio
{
    static class EnumExtensions
    {
        static readonly Dictionary<FactorioBuild, string> BuildVersions
            = new Dictionary<FactorioBuild, string>
            {
                { FactorioBuild.Alpha, "alpha" },
                { FactorioBuild.Demo, "demo" },
                { FactorioBuild.Headless, "headless" }
            };

        static readonly Dictionary<Platform, string> Platforms
            = new Dictionary<Platform, string>
            {
                { Platform.Win64, "win64" },
                { Platform.Win64Manual, "win64-manual" },
                { Platform.OSX, "osx" },
                { Platform.Linux64, "linux64" }
            };

        static readonly Dictionary<UpdatePlatform, string> UpdatePlatforms
            = new Dictionary<UpdatePlatform, string>
            {
                { UpdatePlatform.Win64, "core-win64" },
                { UpdatePlatform.Mac, "core-mac" },
                { UpdatePlatform.Linux64, "core-linux64" },
                { UpdatePlatform.Headless64, "core-linux_headless64" }
            };

        public static string ToActualString(this FactorioBuild buildVersion) => BuildVersions[buildVersion];

        public static string ToActualString(this Platform platform) => Platforms[platform];

        public static string ToActualString(this UpdatePlatform platform) => UpdatePlatforms[platform];
    }
}
