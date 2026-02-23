using CategoryMicroservice.Api.Database;
using CategoryMicroservice.Api.Exceptions;
using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services.CategoryServices;
using Microsoft.EntityFrameworkCore.Storage;

namespace CategoryMicroservice.Api.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public ICategoryRepository<Category> CategoryRepository { get; }
        public ICategoryRepository<Subcategory> SubcategoryRepository { get; }
        private readonly ApplicationDbContext context;
        private IDbContextTransaction? transaction;

        public UnitOfWork(ICategoryRepository<Category> categoryRepository, ICategoryRepository<Subcategory> subcategoryRepository,
            ApplicationDbContext context, IDbContextTransaction transaction)
        {
            this.context = context;
            this.transaction = transaction;
            CategoryRepository = categoryRepository;
            SubcategoryRepository = subcategoryRepository;
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
            if(transaction == null)
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
