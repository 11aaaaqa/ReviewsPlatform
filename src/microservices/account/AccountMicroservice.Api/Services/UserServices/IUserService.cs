using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Models.ReturnModels;

namespace AccountMicroservice.Api.Services.UserServices
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByUserNameAsync(string userName);
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetUsersByUserIds(List<Guid> userIds);
        Task<List<UserReturnModel>> GetUsersByRoleAsync(Role role, int pageSize, int pageNumber);
        Task<List<UserReturnModel>> GetUsersByRoleAsync(List<Role> roles, int pageSize, int pageNumber);
        Task AddUserAsync(User user);
        void UpdateUser(User user);
    }
}
