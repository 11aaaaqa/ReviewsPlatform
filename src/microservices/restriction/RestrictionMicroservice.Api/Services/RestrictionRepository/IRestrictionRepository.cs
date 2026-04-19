using RestrictionMicroservice.Api.Models.Business;

namespace RestrictionMicroservice.Api.Services.RestrictionRepository
{
    public interface IRestrictionRepository
    {
        Task<Restriction?> GetByIdAsync(Guid id);
        Task<Restriction?> GetActiveRestrictionByRestrictedUserIdAsync(Guid restrictedUserId);
        Task<List<Restriction>> GetAllByRestrictedUserIdAsync(Guid restrictedUserId, int pageNumber, int pageSize);
        Task<List<Restriction>> GetAllByRestrictingUserIdAsync(Guid restrictingUserId, int pageNumber, int pageSize);
        Task<List<Restriction>> GetAllByRestrictingUserIdAsync(string query, Guid restrictingUserId, int pageNumber, int pageSize);
        Task<List<Restriction>> GetAllAsync(int pageNumber, int pageSize);
        Task<List<Restriction>> GetAllAsync(string query, int pageNumber, int pageSize);
        Task AddAsync(Restriction restriction);
        void Update(Restriction restriction);
    }
}
