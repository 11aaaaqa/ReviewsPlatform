using AccountMicroservice.Api.Models.Business;

namespace AccountMicroservice.Api.Services.Roles_services
{
    public interface IRoleService
    {
        Task<Role> GetRoleByIdAsync(Guid roleId);
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<List<Role>> GetAllRolesAsync();
    }
}
