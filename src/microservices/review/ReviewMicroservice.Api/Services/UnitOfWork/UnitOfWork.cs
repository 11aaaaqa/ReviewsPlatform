using Microsoft.EntityFrameworkCore.Storage;
using ReviewMicroservice.Api.Database;
using ReviewMicroservice.Api.Exceptions;
using ReviewMicroservice.Api.Services.ReviewServices;
using ReviewMicroservice.Api.Services.ReviewServices.ReactionServices.Repository;

namespace ReviewMicroservice.Api.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public IReviewRepository ReviewRepository { get; }
        public IReactionRepository ReactionRepository { get; }

        private readonly ApplicationDbContext context;
        private IDbContextTransaction? transaction;

        public UnitOfWork(ApplicationDbContext context, IReviewRepository reviewRepository, IReactionRepository reactionRepository)
        {
            this.context = context;
            ReviewRepository = reviewRepository;
            ReactionRepository = reactionRepository;
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

        public async Task CommitTransactionAsync()
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
