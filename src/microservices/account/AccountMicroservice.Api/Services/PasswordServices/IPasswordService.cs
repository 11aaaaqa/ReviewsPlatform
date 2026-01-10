using AccountMicroservice.Api.Models.General;

namespace AccountMicroservice.Api.Services.PasswordServices
{
    public interface IPasswordService
    {
        FormatHashResult HashPassword(string password);
        bool CheckPassword(byte[] originalPasswordHash, byte[] salt, string userPassword);
    }
}
