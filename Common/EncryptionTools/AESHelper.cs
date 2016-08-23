using System.Security.Cryptography;
using System.Text;

namespace Common.EncryptionTools
{
    public static class AESHelper
    {
        public static string Encrypt128ECB(string data, string key)
        {
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var aesAlg = new AesManaged
            {
                KeySize = 128,
                Key = keyBytes,
                BlockSize = 128,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.Zeros,
                IV = keyBytes
            };

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            var res = encryptor.TransformFinalBlock(Encoding.ASCII.GetBytes(data), 0, data.Length);

            return StringUtils.GetBytesToHexString(res).ToLower();
        }
    }
}
