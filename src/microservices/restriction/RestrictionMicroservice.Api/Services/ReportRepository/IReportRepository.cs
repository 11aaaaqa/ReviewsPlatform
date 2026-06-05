using RestrictionMicroservice.Api.Models.Business;

namespace RestrictionMicroservice.Api.Services.ReportRepository
{
    public interface IReportRepository
    {
        Task<Report?> GetByIdAsync(Guid id);
        Task<List<Report>> GetAllAsync(int pageSize, int pageNumber);
        Task<List<Report>> GetAllAsync(Guid reportingUserId, int pageSize, int pageNumber);
        Task AddAsync(Report model);
        Task RemoveAsync(Guid reportId);
        Task RemoveAllByReportOnEntityIdAsync(Guid reportOnEntityId);
        Task RemoveAllByReportingUserIdAsync(Guid userId);
    }
}
