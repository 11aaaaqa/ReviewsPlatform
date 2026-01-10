using AccountMicroservice.Api.Models.Business;

namespace AccountMicroservice.Api.Services.RolesServices
{
    public interface IRoleService
    {
        Task<Role> GetRoleByIdAsync(Guid roleId);
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<List<Role>> GetAllRolesAsync();
    }
}
