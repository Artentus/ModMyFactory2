using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal static class FileInfoExtensions
    {
        private static async Task<byte[]> ComputeSHA1Async(Stream stream)
        {
            const int BufferSize = 16384;

            using var sha1 = SHA1.Create();
            sha1.Initialize();

            byte[] buffer = new byte[BufferSize];
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, BufferSize);
                if (stream.Position >= stream.Length)
                {
                    sha1.TransformFinalBlock(buffer, 0, bytesRead);
                    break;
                }
                else
                {
                    sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                }
            }

            return sha1.Hash;
        }

        public static async Task<SHA1Hash> ComputeSHA1Async(this FileInfo file)
        {
            using var stream = file.OpenRead();
            byte[] bytes = await ComputeSHA1Async(stream);
            return new SHA1Hash(bytes);
        }
    }
}
