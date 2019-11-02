using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Luid
    {
        public uint Low;
        public int High;
    }
}
