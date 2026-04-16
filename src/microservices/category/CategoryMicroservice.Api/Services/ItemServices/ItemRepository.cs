using CategoryMicroservice.Api.Database;
using CategoryMicroservice.Api.Enums;
using CategoryMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace CategoryMicroservice.Api.Services.ItemServices
{
    public class ItemRepository(ApplicationDbContext context) : IItemRepository
    {
        public async Task<Item?> GetByIdAsync(Guid itemId)
            => await context.Items.SingleOrDefaultAsync(x => x.Id == itemId);

        public async Task<List<Item>> GetByNameAsync(string name)
            => await context.Items.Where(x => x.Name.ToLower() == name.ToLower()).ToListAsync();

        public async Task<List<Item>> GetAllBySubcategoryIdAsync(Guid subcategoryId, int pageNumber, int pageSize)
        {
            var items = await context.Items
                .Where(x => x.Status == ItemStatus.Verified)
                .Where(x => x.SubcategoryId == subcategoryId)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return items;
        }

        public async Task<List<Item>> FindByContainedCharactersAsync(string name, int pageNumber, int pageSize)
        {
            var items = await context.Items
                .Where(x => x.Status == ItemStatus.Verified)
                .Where(x => x.Name.ToLower().Contains(name.ToLower()))
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return items;
        }

        public async Task<List<Item>> FindByContainedCharactersAsync(Guid subcategoryId, string name, int pageNumber, int pageSize)
        {
            var items = await context.Items
                .Where(x => x.Status == ItemStatus.Verified)
                .Where(x => x.SubcategoryId == subcategoryId)
                .Where(x => x.Name.ToLower().Contains(name.ToLower()))
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return items;
        }

        public async Task AddAsync(Item item)
        {
            await context.Items.AddAsync(item);
        }

        public void Update(Item item)
        {
            context.Items.Update(item);
        }

        public async Task RemoveAsync(Guid itemId)
        {
            var item = await context.Items.SingleOrDefaultAsync(x => x.Id == itemId);
            if (item == null)
                throw new ArgumentException("Item with current identifier does not exist");

            context.Items.Remove(item);
        }
    }
}
