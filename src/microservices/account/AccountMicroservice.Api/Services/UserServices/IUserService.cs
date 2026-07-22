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
        Task<GetUsersModel> GetUsersAsync(string? query, UserSort userSort, int pageSize, int pageNumber);
        Task<GetUsersModel> GetUsersByRoleAsync(string? query, Guid roleId, UserSort userSort, int pageSize, int pageNumber);
        Task<GetUsersModel> GetUsersByRoleAsync(string? query, List<Guid> roleIds, UserSort userSort, int pageSize, int pageNumber);
        Task AddUserAsync(User user);
        void UpdateUser(User user);
    }
}
