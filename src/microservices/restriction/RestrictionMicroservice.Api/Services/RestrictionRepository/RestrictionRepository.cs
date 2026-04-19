using Microsoft.EntityFrameworkCore;
using RestrictionMicroservice.Api.Database;
using RestrictionMicroservice.Api.Models.Business;

namespace RestrictionMicroservice.Api.Services.RestrictionRepository
{
    public class RestrictionRepository(ApplicationDbContext context) : IRestrictionRepository
    {
        public async Task<Restriction?> GetByIdAsync(Guid id)
            => await context.Restrictions.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<Restriction?> GetActiveRestrictionByRestrictedUserIdAsync(Guid restrictedUserId)
            => await context.Restrictions.Where(x => x.RestrictedUserId == restrictedUserId && !x.IsDisabled)
                .FirstOrDefaultAsync(x => x.ExpiryTime > DateTime.UtcNow);

        public async Task<List<Restriction>> GetAllByRestrictedUserIdAsync(Guid restrictedUserId, int pageNumber,
            int pageSize)
            => await context.Restrictions.Where(x => x.RestrictedUserId == restrictedUserId).OrderBy(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task<List<Restriction>> GetAllByRestrictingUserIdAsync(Guid restrictingUserId, int pageNumber, int pageSize)
            => await context.Restrictions.Where(x => x.RestrictingUserId == restrictingUserId).OrderBy(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task<List<Restriction>> GetAllByRestrictingUserIdAsync(string query, Guid restrictingUserId, int pageNumber, int pageSize)
            => await context.Restrictions.Where(x => x.RestrictingUserId == restrictingUserId)
                .Where(x => x.Reason.ToLower().Contains(query.ToLower()))
                .OrderBy(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task<List<Restriction>> GetAllAsync(int pageNumber, int pageSize)
            => await context.Restrictions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task<List<Restriction>> GetAllAsync(string query, int pageNumber, int pageSize)
            => await context.Restrictions.Where(x => x.Reason.ToLower().Contains(query.ToLower()))
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task AddAsync(Restriction restriction)
        {
            await context.Restrictions.AddAsync(restriction);
        }

        public void Update(Restriction restriction)
        {
            context.Restrictions.Update(restriction);
        }
    }
}
