using AccountMicroservice.Api.Models.Business;

namespace AccountMicroservice.Api.Services.UserServices
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByUserNameAsync(string userName);
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetUsersByUserIds(List<Guid> userIds);
        Task AddUserAsync(User user);
        void UpdateUser(User user);
    }
}
