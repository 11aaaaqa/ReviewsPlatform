using Microsoft.EntityFrameworkCore.Storage;
using RestrictionMicroservice.Api.Database;
using RestrictionMicroservice.Api.Exceptions;
using RestrictionMicroservice.Api.Services.ReportRepository;
using RestrictionMicroservice.Api.Services.RestrictionRepository;

namespace RestrictionMicroservice.Api.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public IReportRepository ReportRepository { get; }
        public IRestrictionRepository RestrictionRepository { get; }

        private readonly ApplicationDbContext context;
        private IDbContextTransaction? transaction;
        public UnitOfWork(ApplicationDbContext context, IReportRepository reportRepository, IRestrictionRepository restrictionRepository)
        {
            this.context = context;
            ReportRepository = reportRepository;
            RestrictionRepository = restrictionRepository;
        }

        public async Task CompleteAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (transaction != null)
                throw new TransactionAlreadyBeganException();
            transaction = await context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (transaction == null)
                throw new TransactionHasNotBegunException();

            await context.SaveChangesAsync();
            await context.Database.CommitTransactionAsync();

            Dispose();
        }

        public async Task RollbackTransactionAsync()
        {
            if (transaction == null)
                throw new TransactionHasNotBegunException();

            try
            {
                await context.Database.RollbackTransactionAsync();
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            transaction?.Dispose();
            transaction = null;
        }
    }
}
