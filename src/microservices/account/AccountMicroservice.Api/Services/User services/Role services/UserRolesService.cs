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
            var user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new ArgumentException("User with current id does not exist");

            var role = await context.Roles.SingleOrDefaultAsync(x => x.Id == roleId);
            if (role == null)
                throw new ArgumentException("Role with current id does not exist");

            await context.UserRoles.AddAsync(new UserRole { RoleId = roleId, UserId = userId });
            await context.SaveChangesAsync();
        }

        public async Task AddUserToRolesRangeAsync(Guid userId, ICollection<Guid> roleIds)
        {
            var user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new ArgumentException("User with current id does not exist");

            roleIds = roleIds.Distinct().ToList();

            var existingRoleIds = await context.Roles.Select(x => x.Id).ToListAsync();
            if (roleIds.Except(existingRoleIds).Any())
                throw new ArgumentException("Role with current id does not exist");

            var userRoleIds = await context.UserRoles.Where(x => x.UserId == userId)
                .Select(x => x.RoleId)
                .ToListAsync();
            var roleIdsToAdd = roleIds.Except(userRoleIds).ToList();

            List<UserRole> rolesToAdd = new();
            foreach (var roleIdToAdd in roleIdsToAdd)
            {
                rolesToAdd.Add(new UserRole { RoleId = roleIdToAdd, UserId = user.Id });
            }

            context.UserRoles.AddRange(rolesToAdd);
            await context.SaveChangesAsync();
        }

        public async Task RemoveUserRoleAsync(Guid userId, Guid roleId)
        {
            var userRoleToRemove = await context.UserRoles.SingleOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);
            if (userRoleToRemove == null)
                throw new ArgumentException("User role does not exist");

            context.UserRoles.Remove(userRoleToRemove);
            await context.SaveChangesAsync();
        }

        public async Task RemoveUserRolesRangeAsync(Guid userId, ICollection<Guid> roleIds)
        {
            roleIds = roleIds.Distinct().ToList();

            var userRolesToRemove = await context.UserRoles
                .Where(x => x.UserId == userId && roleIds.Contains(x.RoleId))
                .ToListAsync();
            context.UserRoles.RemoveRange(userRolesToRemove);
            await context.SaveChangesAsync();
        }
    }
}
