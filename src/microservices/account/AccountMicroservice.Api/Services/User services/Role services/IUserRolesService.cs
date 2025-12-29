using AccountMicroservice.Api.Models.Business;

namespace AccountMicroservice.Api.Services.User_services.Role_services
{
    public interface IUserRolesService
    {
        Task<List<Role>> GetUserRolesAsync(Guid userId);
        Task AddUserToRoleAsync(Guid userId, Guid roleId);
        Task AddUserToRolesRangeAsync(Guid userId, ICollection<Guid> roleIds);
        Task RemoveUserRoleAsync(Guid userId, Guid roleId);
        Task RemoveUserRolesRangeAsync(Guid userId, ICollection<Guid> roleIds);
    }
}
