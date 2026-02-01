using AccountMicroservice.Api.Enums;
using AccountMicroservice.Api.Models.Business;

namespace AccountMicroservice.Api.Services.UserServices.EmailTokenServices
{
    public interface IUserEmailTokenRepository
    {
        Task<List<UserEmailToken>> GetEmailTokensByUserIdAsync(Guid userId);
        Task<List<UserEmailToken>> GetEmailTokensByPurposeAsync(Guid userId, EmailTokenPurpose purpose);
        Task<UserEmailToken?> GetAsync(Guid userId, string token, EmailTokenPurpose purpose);
        Task<string> AddAsync(Guid userId, EmailTokenPurpose purpose);
        Task RemoveAsync(Guid userId, string token);
        Task RemoveRangeAsync(Guid userId, List<UserEmailToken> emailTokens);
        Task RemoveAllByPurposeAsync(Guid userId, EmailTokenPurpose purpose);
    }
}
