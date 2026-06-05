using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services.CategoryServices;
using CategoryMicroservice.Api.Services.ItemServices;

namespace CategoryMicroservice.Api.Services.UnitOfWork
{
    public interface IUnitOfWork
    {
        ICategoryRepository<Category> CategoryRepository { get; }
        ICategoryRepository<Subcategory> SubcategoryRepository { get; }
        IItemRepository ItemRepository { get; }

        Task CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
