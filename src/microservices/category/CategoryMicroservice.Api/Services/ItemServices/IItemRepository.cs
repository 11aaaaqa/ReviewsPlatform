using CategoryMicroservice.Api.Models.Business;

namespace CategoryMicroservice.Api.Services.ItemServices
{
    public interface IItemRepository
    {
        Task<Item?> GetByIdAsync(Guid itemId);
        Task<List<Item>> GetByNameAsync(string name);
        Task<List<Item>> GetAllBySubcategoryIdAsync(Guid subcategoryId, int pageNumber, int pageSize);
        Task<List<Item>> FindByContainedCharactersAsync(string name, int pageNumber, int pageSize);
        Task<List<Item>> FindByContainedCharactersAsync(Guid subcategoryId, string name, int pageNumber, int pageSize);
        Task AddAsync(Item item);
        void Update(Item item);
        Task RemoveAsync(Guid itemId);
    }
}
