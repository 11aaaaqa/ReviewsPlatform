namespace CategoryMicroservice.Api.Services.CategoryServices
{
    public interface ICategoryRepository<T>
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<List<T>> GetAllAsync();
        Task<T?> FindByNameAsync(string name);
        Task<List<T>> FindByContainedCharactersInNameAsync(string name);
        Task AddAsync(T model);
        Task UpdateAsync(T model);
        Task RemoveAsync(Guid id);
    }
}
