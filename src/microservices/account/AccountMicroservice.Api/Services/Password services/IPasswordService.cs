using AccountMicroservice.Api.Models.General;

namespace AccountMicroservice.Api.Services.Password_services
{
    public interface IPasswordService
    {
        FormatHashResult HashPassword(string password);
        bool CheckPassword(byte[] originalPasswordHash, byte[] salt, string userPassword);
    }
}
