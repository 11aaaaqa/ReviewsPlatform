using CategoryMicroservice.Api.Database;
using CategoryMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace CategoryMicroservice.Api.Services.CategoryServices
{
    public class SubcategoryRepository(ApplicationDbContext context) : ICategoryRepository<Subcategory>
    {
        public async Task<Subcategory?> GetByIdAsync(Guid id)
            => await context.Subcategories.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Subcategory>> GetAllAsync()
            => await context.Subcategories.ToListAsync();

        public async Task<Subcategory?> FindByNameAsync(string name)
            => await context.Subcategories.SingleOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

        public async Task<List<Subcategory>> FindByContainedCharactersInNameAsync(string name)
            => await context.Subcategories.Where(x => x.Name.ToLower().Contains(name.ToLower())).ToListAsync();

        public async Task AddAsync(Subcategory model)
        {
            await context.Subcategories.AddAsync(model);
        }

        public async Task UpdateAsync(Subcategory model)
        {
            context.Subcategories.Update(model);
        }

        public async Task RemoveAsync(Guid id)
        {
            var subcategory = await context.Subcategories.SingleOrDefaultAsync(x => x.Id == id);
            if (subcategory == null)
                throw new ArgumentException("Subcategory with current identifier does not exist");

            context.Subcategories.Remove(subcategory);
        }
    }
}
