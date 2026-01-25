using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace AccountMicroservice.Api.Services.UserServices
{
    public class UserService(ApplicationDbContext context) : IUserService
    {
        public async Task<User?> GetUserByIdAsync(Guid userId)
            => await context.Users.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id == userId);

        public async Task<User?> GetUserByUserNameAsync(string userName)
            => await context.Users.Include(x => x.Roles)
                .SingleOrDefaultAsync(x => x.UserName.ToLower() == userName.ToLower());

        public async Task<User?> GetUserByEmailAsync(string email)
            => await context.Users.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Email == email);

        public async Task AddUserAsync(User user)
        {
            await context.Users.AddAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            context.Users.Update(user);
        }
    }
}
