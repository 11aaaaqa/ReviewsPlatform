using AccountMicroservice.Api.Enums.SortEnums;
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
        Task<List<UserReturnModel>> GetUsersAsync(string? query, UserSort userSort, int pageSize, int pageNumber);
        Task<List<UserReturnModel>> GetUsersByRoleAsync(string? query, Guid roleId, UserSort userSort, int pageSize, int pageNumber);
        Task<List<UserReturnModel>> GetUsersByRoleAsync(string? query, List<Guid> roleIds, UserSort userSort, int pageSize, int pageNumber);
        Task<int> GetUsersCountAsync();
        Task AddUserAsync(User user);
        void UpdateUser(User user);
    }
}
