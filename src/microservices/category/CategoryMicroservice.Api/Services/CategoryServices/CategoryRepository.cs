using CategoryMicroservice.Api.Database;
using CategoryMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace CategoryMicroservice.Api.Services.CategoryServices
{
    public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository<Category>
    {
        public async Task<Category?> GetByIdAsync(Guid id)
            => await context.Categories.Include(x => x.Subcategories).SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Category>> GetAllAsync()
            => await context.Categories.Include(x => x.Subcategories).ToListAsync();

        public async Task<Category?> FindByNameAsync(string name)
            => await context.Categories.Include(x => x.Subcategories)
                .SingleOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

        public async Task<List<Category>> FindByContainedCharactersInNameAsync(string name)
            => await context.Categories.Include(x => x.Subcategories)
                .Where(x => x.Name.ToLower().Contains(name.ToLower())).ToListAsync();

        public async Task AddAsync(Category model)
        {
            await context.Categories.AddAsync(model);
        }

        public async Task UpdateAsync(Category model)
        {
            context.Categories.Update(model);
        }

        public async Task RemoveAsync(Guid id)
        {
            var category = await context.Categories.SingleOrDefaultAsync(x => x.Id == id);
            if (category == null)
                throw new ArgumentException("Category with current identifier does not exist");

            context.Categories.Remove(category);
        }
    }
}
