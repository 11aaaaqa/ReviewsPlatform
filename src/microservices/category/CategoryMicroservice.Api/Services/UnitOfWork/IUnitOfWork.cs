using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services.CategoryServices;

namespace CategoryMicroservice.Api.Services.UnitOfWork
{
    public interface IUnitOfWork
    {
        ICategoryRepository<Category> CategoryRepository { get; }
        ICategoryRepository<Subcategory> SubcategoryRepository { get; }

        Task CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
