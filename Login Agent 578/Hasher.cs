using System.Security.Cryptography;
using System.Text;

namespace Login_Agent_578
{
    internal enum HashType
    {
        NONE,
        MD5,
        BCrypt
    }
    internal static class Hasher
    {
        /// <summary>
        /// 비밀번호 체크
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="hashed"></param>
        /// <param name="algo"></param>
        /// <returns></returns>
        internal static bool Verify(string plaintext, string hashed, HashType algo)
        {
            if (algo == HashType.BCrypt)
                return BCrypt.Verify(plaintext, hashed);
            else
                return hashed == HashString(plaintext, algo);
        }

        private static string HashString(string hashMe, HashType algo)
        {
            switch (algo)
            {
                case HashType.NONE:
                    return hashMe;
                case HashType.MD5:
                    return MakeHashString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(hashMe)));
                case HashType.BCrypt:
                    return BCrypt.HashPassword(hashMe, BCrypt.GenerateSalt());
                default:
                    return "";
            }
        }

        private static string MakeHashString(byte[] hash)
        {
            StringBuilder s = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
                s.Append(b.ToString("X2"));

            return s.ToString().ToLower();
        }
    }
}