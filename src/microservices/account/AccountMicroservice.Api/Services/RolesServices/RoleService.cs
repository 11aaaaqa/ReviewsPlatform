using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace AccountMicroservice.Api.Services.RolesServices
{
    public class RoleService(ApplicationDbContext context) : IRoleService
    {
        public async Task<Role> GetRoleByIdAsync(Guid roleId)
            => await context.Roles.SingleAsync(x => x.Id == roleId);

        public async Task<Role> GetRoleByNameAsync(string roleName)
            => await context.Roles.SingleAsync(x => x.Name == roleName);

        public async Task<List<Role>> GetAllRolesAsync()
            => await context.Roles.ToListAsync();
    }
}
