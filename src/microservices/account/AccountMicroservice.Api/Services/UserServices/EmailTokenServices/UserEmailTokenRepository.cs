using System.Buffers.Text;
using System.Security.Cryptography;
using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Enums;
using AccountMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace AccountMicroservice.Api.Services.UserServices.EmailTokenServices
{
    public class UserEmailTokenRepository(ApplicationDbContext context) : IUserEmailTokenRepository
    {
        public async Task<List<UserEmailToken>> GetEmailTokensByUserIdAsync(Guid userId)
            => await context.UserEmailTokens.Where(x => x.UserId == userId).ToListAsync();

        public async Task<List<UserEmailToken>> GetEmailTokensByPurposeAsync(Guid userId, EmailTokenPurpose purpose)
            => await context.UserEmailTokens
                .Where(x => x.UserId == userId)
                .Where(x => x.TokenPurpose == purpose)
                .ToListAsync();

        public async Task<UserEmailToken?> GetAsync(Guid userId, string token, EmailTokenPurpose purpose)
            => await context.UserEmailTokens
                .SingleOrDefaultAsync(x => x.UserId == userId && x.TokenPurpose == purpose && x.Token == token);

        public async Task<string> AddAsync(Guid userId, EmailTokenPurpose purpose)
        {
            byte[] tokenBytes = new byte[48];

            using RandomNumberGenerator generator = RandomNumberGenerator.Create();
            generator.GetBytes(tokenBytes);
            
            string tokenStr = Base64Url.EncodeToString(tokenBytes);
            var emailToken = new UserEmailToken
            {
                ExpiryTime = DateTime.UtcNow.AddMinutes(10), TokenPurpose = purpose,
                UserId = userId, Token = tokenStr
            };
            await context.UserEmailTokens.AddAsync(emailToken);
            return emailToken.Token;
        }

        public async Task RemoveAsync(Guid userId, string token)
        {
            var emailToken = await context.UserEmailTokens.SingleOrDefaultAsync(x => x.Token == token && x.UserId == userId);
            if (emailToken == null)
                throw new ArgumentException("Token does not exist");

            context.UserEmailTokens.Remove(emailToken);
        }

        public async Task RemoveRangeAsync(Guid userId, List<UserEmailToken> emailTokens)
        {
            var emailTokensToRemove = await context.UserEmailTokens
                .Where(x => emailTokens.Contains(x) && x.UserId == userId)
                .ToListAsync();
            context.UserEmailTokens.RemoveRange(emailTokensToRemove);
        }

        public async Task RemoveAllByPurposeAsync(Guid userId, EmailTokenPurpose purpose)
        {
            var emailTokensToRemove = await context.UserEmailTokens
                .Where(x => x.UserId == userId && x.TokenPurpose == purpose).ToListAsync();

            if (emailTokensToRemove.Count > 0)
            {
                context.UserEmailTokens.RemoveRange(emailTokensToRemove);
            }
        }

        public async Task RemoveAllExpiredEmailTokensAsync()
        {
            var emailTokensToDelete = await context.UserEmailTokens
                .Where(x => x.ExpiryTime < DateTime.UtcNow).ToListAsync();
            context.UserEmailTokens.RemoveRange(emailTokensToDelete);
        }
    }
}
