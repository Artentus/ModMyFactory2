using ModMyFactory.BaseTypes;
using System.IO;

namespace ModMyFactory.ModSettings.Serialization
{
    static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, AccurateVersion version)
        {
            writer.Write(version.Main);
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Revision);
        }
    }
}
