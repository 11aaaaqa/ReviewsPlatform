using Microsoft.EntityFrameworkCore;
using RestrictionMicroservice.Api.Database;
using RestrictionMicroservice.Api.Models.Business;

namespace RestrictionMicroservice.Api.Services.ReportRepository
{
    public class ReportRepository(ApplicationDbContext context) : IReportRepository
    {
        public async Task<Report?> GetByIdAsync(Guid id)
            => await context.Reports.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Report>> GetAllAsync(int pageSize, int pageNumber)
            => await context.Reports.OrderBy(x => x.CreatedAt).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task<List<Report>> GetAllAsync(Guid reportingUserId, int pageSize, int pageNumber)
            => await context.Reports.Where(x => x.ReportingUserId == reportingUserId)
                .OrderByDescending(x => x.CreatedAt).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task AddAsync(Report model)
        {
            await context.Reports.AddAsync(model);
        }

        public async Task RemoveAsync(Guid reportId)
        {
            var report = await context.Reports.SingleOrDefaultAsync(x => x.Id == reportId);
            if (report == null)
                throw new ArgumentException("Report with current identifier does not exist");

            context.Reports.Remove(report);
        }

        public async Task RemoveAllByReportOnEntityIdAsync(Guid reportOnEntityId)
        {
            var reportToRemove = await context.Reports.Where(x => x.ReportOnEntityId == reportOnEntityId).ToListAsync();
            context.Reports.RemoveRange(reportToRemove);
        }

        public async Task RemoveAllByReportingUserIdAsync(Guid userId)
        {
            var reports = await context.Reports.Where(x => x.ReportingUserId == userId).ToListAsync();
            if(reports.Count > 0)
                context.Reports.RemoveRange(reports);
        }
    }
}
