using System;
using System.Security.Cryptography;

namespace ShopManagement.Infrastructure
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, SaltSize, Iterations))
            {
                var salt = deriveBytes.Salt;
                var hash = deriveBytes.GetBytes(HashSize);
                return string.Format("{0}:{1}", Convert.ToBase64String(salt), Convert.ToBase64String(hash));
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var parts = storedHash.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                var actualHash = deriveBytes.GetBytes(HashSize);
                return SlowEquals(expectedHash, actualHash);
            }
        }

        private static bool SlowEquals(byte[] left, byte[] right)
        {
            var difference = left.Length ^ right.Length;
            for (var index = 0; index < left.Length && index < right.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }
    }
}