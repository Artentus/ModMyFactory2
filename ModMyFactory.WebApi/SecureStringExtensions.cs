using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace ModMyFactory.WebApi
{
    static class SecureStringExtensions
    {
        public static unsafe byte[] ToBytes(this SecureString secureString, Encoding encoding)
        {
            byte[] bytes;
            IntPtr bStr = IntPtr.Zero;
            try
            {
                bStr = Marshal.SecureStringToBSTR(secureString);
                char* charPointer = (char*)bStr.ToPointer();

                int byteCount = encoding.GetByteCount(charPointer, secureString.Length);
                bytes = new byte[byteCount];
                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

                try
                {
                    byte* bytePointer = (byte*)handle.AddrOfPinnedObject().ToPointer();
                    encoding.GetBytes(charPointer, secureString.Length, bytePointer, byteCount);
                }
                finally
                {
                    handle.Free();
                }
            }
            finally
            {
                if (bStr != IntPtr.Zero) Marshal.ZeroFreeBSTR(bStr);
            }

            return bytes;
        }

        public static byte[] ToBytes(this SecureString secureString)
        {
            return ToBytes(secureString, Encoding.UTF8);
        }
    }
}
