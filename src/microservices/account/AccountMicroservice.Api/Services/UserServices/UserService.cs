using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Models.ReturnModels;
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

        public async Task<List<User>> GetUsersByUserIds(List<Guid> userIds)
            => await context.Users.Where(x => userIds.Contains(x.Id)).ToListAsync();

        public async Task<List<UserReturnModel>> GetUsersByRoleAsync(Role role, int pageSize, int pageNumber)
        {
            var users = await context.UserRoles
                .Where(x => x.RoleId == role.Id)
                .Join(
                    context.Users,
                    ur => ur.UserId,
                    u => u.Id, (userRole, user) => new UserReturnModel
                    {
                        Id = user.Id, Email = user.Email, Roles = user.Roles, AvatarSource = user.AvatarSource,
                        IsAvatarDefault = user.IsAvatarDefault, IsEmailVerified = user.IsEmailVerified,
                        RegistrationDate = user.RegistrationDate, UserName = user.UserName
                    })
                .Include(x => x.Roles)
                .OrderBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return users;
        }

        public async Task<List<UserReturnModel>> GetUsersByRoleAsync(List<Role> roles, int pageSize, int pageNumber)
        {
            List<Guid> roleIds = roles.Select(x => x.Id).ToList();
            var users = await context.UserRoles
                .Where(x => roleIds.Contains(x.RoleId))
                .Join(
                    context.Users,
                    ur => ur.UserId,
                    u => u.Id, (userRole, user) => new UserReturnModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Roles = user.Roles,
                        AvatarSource = user.AvatarSource,
                        IsAvatarDefault = user.IsAvatarDefault,
                        IsEmailVerified = user.IsEmailVerified,
                        RegistrationDate = user.RegistrationDate,
                        UserName = user.UserName
                    })
                .Distinct()
                .Include(x => x.Roles)
                .OrderBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return users;
        }

        public async Task AddUserAsync(User user)
        {
            await context.Users.AddAsync(user);
        }

        public void UpdateUser(User user)
        {
            context.Users.Update(user);
        }
    }
}
