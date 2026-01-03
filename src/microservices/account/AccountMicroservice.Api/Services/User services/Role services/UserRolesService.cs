using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace AccountMicroservice.Api.Services.User_services.Role_services
{
    public class UserRolesService(ApplicationDbContext context) : IUserRolesService
    {
        public async Task<List<Role>> GetUserRolesAsync(Guid userId)
            => await context.Users
                .Where(x => x.Id == userId)
                .Include(x => x.Roles)
                .SelectMany(x => x.Roles)
                .ToListAsync();

        public async Task AddUserToRoleAsync(Guid userId, Guid roleId)
        {
            await context.UserRoles.AddAsync(new UserRole { RoleId = roleId, UserId = userId });
            await context.SaveChangesAsync();
        }

        public async Task AddUserToRolesRangeAsync(Guid userId, ICollection<Guid> roleIds)
        {
            List<UserRole> rolesToAdd = new();
            foreach (var roleIdToAdd in roleIds)
            {
                rolesToAdd.Add(new UserRole { RoleId = roleIdToAdd, UserId = userId });
            }

            context.UserRoles.AddRange(rolesToAdd);
            await context.SaveChangesAsync();
        }

        public async Task RemoveUserRoleAsync(Guid userId, Guid roleId)
        {
            var userRoleToRemove = await context.UserRoles.SingleAsync(x => x.UserId == userId && x.RoleId == roleId);
            context.UserRoles.Remove(userRoleToRemove);
            await context.SaveChangesAsync();
        }

        public async Task RemoveUserRolesRangeAsync(Guid userId, ICollection<Guid> roleIds)
        {
            var userRolesToRemove = await context.UserRoles
                .Where(x => x.UserId == userId && roleIds.Contains(x.RoleId))
                .ToListAsync();
            context.UserRoles.RemoveRange(userRolesToRemove);
            await context.SaveChangesAsync();
        }
    }
}
