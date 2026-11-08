using System;
using System.Security.Cryptography;
using System.Text;

namespace LMS.Infrastructure.Services.Agora.Token
{
    public static class DynamicKeyUtil
    {
        public static byte[] EncodeHMAC(string key, byte[] message, string alg = "SHA256")
        {
            return EncodeHMAC(Encoding.UTF8.GetBytes(key), message, alg);
        }

        public static byte[] EncodeHMAC(byte[] keyBytes, byte[] textBytes, string alg = "SHA256")
        {
            using var hmac = alg.ToUpperInvariant() switch
            {
                "MD5" => (HMAC)new HMACMD5(keyBytes),
                "SHA1" => new HMACSHA1(keyBytes),
                "SHA384" => new HMACSHA384(keyBytes),
                "SHA512" => new HMACSHA512(keyBytes),
                _ => new HMACSHA256(keyBytes)
            };

            return hmac.ComputeHash(textBytes);
        }

        public static string BytesToHex(byte[] inData)
        {
            var builder = new StringBuilder(inData.Length * 2);
            foreach (byte b in inData)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}

