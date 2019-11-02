using ModMyFactory.BaseTypes;
using System.IO;

namespace ModMyFactory.ModSettings.Serialization
{
    static class BinaryReaderExtensions
    {
        public static AccurateVersion ReadVersion(this BinaryReader reader)
        {
            var main = reader.ReadUInt16();
            var major = reader.ReadUInt16();
            var minor = reader.ReadUInt16();
            var revisio = reader.ReadUInt16();
            return new AccurateVersion(main, major, minor, revisio);
        }
    }
}
