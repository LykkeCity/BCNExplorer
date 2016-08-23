using System;
using System.Security.Cryptography;

namespace Common.PasswordTools
{
    public interface IPasswordKeeping
    {
        string Salt { get; set; }
        string Hash { get; set; }
    }

    public static class PasswordKeepingUtils
    {
        private static string CalcHash(string password, string salt)
        {

            var cryptoTransformSha1 = new SHA1CryptoServiceProvider();

            var sha1 = cryptoTransformSha1.ComputeHash((password + salt).ToUtf8Bytes());

            return Convert.ToBase64String(sha1);
        }

        public static void SetPassword(this IPasswordKeeping entity, string password)
        {
            entity.Salt = Guid.NewGuid().ToString();
            entity.Hash = CalcHash(password, entity.Salt);
        }

        public static bool CheckPassword(this IPasswordKeeping entity, string password)
        {
            var hash = CalcHash(password, entity.Salt);
            return entity.Hash == hash;
        }

    }

}
