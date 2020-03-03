//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace ModMyFactory.WebApi
{
    internal static class SecureStringExtensions
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
            => ToBytes(secureString, Encoding.UTF8);
    }
}
