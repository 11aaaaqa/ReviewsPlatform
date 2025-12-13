using System.Security.Cryptography;
using System.Text;
using AccountMicroservice.Api.Models.General;
using Konscious.Security.Cryptography;

namespace AccountMicroservice.Api.Services.Password_services
{
    public class PasswordService : IPasswordService
    {
        public FormatHashResult HashPassword(string password)
        {
            byte[] salt = GenerateSecureSalt();

            var hashResult = Hash(password, salt);

            return new FormatHashResult{PasswordHash = hashResult.PasswordHash, Salt = hashResult.Salt};
        }

        public bool CheckPassword(byte[] originalPasswordHash, string userPassword, byte[] salt)
        {
            var hashResult = Hash(userPassword, salt);

            for (int i = 0; i < originalPasswordHash.Length; i++)
            {
                if (originalPasswordHash[i] != hashResult.PasswordHash[i])
                    return false;
            }

            return true;
        }

        private byte[] GenerateSecureSalt(int size = 16)
        {
            var salt = new byte[size];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            return salt;
        }

        private FormatHashResult Hash(string password, byte[] salt)
        {
            using Argon2id argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8;
            argon2.Iterations = 4;
            argon2.MemorySize = 65536;

            byte[] passwordHash = argon2.GetBytes(32);

            return new FormatHashResult {Salt = salt, PasswordHash = passwordHash};
        }
    }
}
