using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace LMS.Infrastructure.Services.Agora.Token
{
    public static class Utils
    {
        public static int GetTimestamp()
        {
            return (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static int RandomInt()
        {
            return Random.Shared.Next();
        }

        public static string Base64Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static byte[] Base64Decode(string data)
        {
            return Convert.FromBase64String(data);
        }

        public static bool IsUUID(string uuid)
        {
            if (string.IsNullOrEmpty(uuid) || uuid.Length != 32)
            {
                return false;
            }

            return Regex.IsMatch(uuid, "^[0-9a-fA-F]{32}$");
        }

        public static byte[] Compress(byte[] data)
        {
            using var outputStream = new MemoryStream();
            using (var zlibStream = new ZLibStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
            {
                zlibStream.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            using var inputStream = new MemoryStream(data);
            using var outputStream = new MemoryStream();
            using (var zlibStream = new ZLibStream(inputStream, CompressionMode.Decompress))
            {
                zlibStream.CopyTo(outputStream);
            }
            return outputStream.ToArray();
        }
    }
}

