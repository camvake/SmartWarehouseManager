using System;
using System.Security.Cryptography;
using System.Text;

namespace SWM.Core.Services
{
    public static class PasswordService
    {
        public static (string Hash, string Salt) HashPassword(string password)
        {
            // Простая реализация для начала
            using (var sha256 = SHA256.Create())
            {
                var salt = Guid.NewGuid().ToString("N");
                var combined = password + salt;
                var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(combined)));
                return (hash, salt);
            }
        }

        public static bool VerifyPassword(string password, string hash, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combined = password + salt;
                var newHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(combined)));
                return newHash == hash;
            }
        }

        public static string GenerateRandomPassword(int length = 8)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}