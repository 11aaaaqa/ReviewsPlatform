using RestrictionMicroservice.Api.Services.ReportRepository;
using RestrictionMicroservice.Api.Services.RestrictionRepository;

namespace RestrictionMicroservice.Api.Services.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IReportRepository ReportRepository { get; }
        IRestrictionRepository RestrictionRepository { get; }

        Task CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackTransactionAsync();
    }
}
