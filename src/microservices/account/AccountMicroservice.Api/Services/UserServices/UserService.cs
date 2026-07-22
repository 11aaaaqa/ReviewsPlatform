using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Enums.SortEnums;
using AccountMicroservice.Api.Models.Business;
using AccountMicroservice.Api.Models.ReturnModels;
using Microsoft.EntityFrameworkCore;

namespace AccountMicroservice.Api.Services.UserServices
{
    public class UserService(ApplicationDbContext context) : IUserService
    {
        public async Task<User?> GetUserByIdAsync(Guid userId)
            => await context.Users.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id == userId);

        public async Task<User?> GetUserByUserNameAsync(string userName)
            => await context.Users.Include(x => x.Roles)
                .SingleOrDefaultAsync(x => x.UserName.ToLower() == userName.ToLower());

        public async Task<User?> GetUserByEmailAsync(string email)
            => await context.Users.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Email == email);

        public async Task<List<User>> GetUsersByUserIds(List<Guid> userIds)
            => await context.Users.Where(x => userIds.Contains(x.Id)).ToListAsync();

        public async Task<GetUsersModel> GetUsersAsync(string? query, UserSort userSort, int pageSize, int pageNumber)
        {
            IQueryable<UserReturnModel> users;
            if (query != null)
            {
                query = query.ToLower();
                users = context.Users
                    .Where(x => x.UserName.ToLower().Contains(query) || x.Email.ToLower().Contains(query))
                    .Select(user => new UserReturnModel
                    {
                        Id = user.Id, Roles = user.Roles, AvatarSource = user.AvatarSource, Email = user.Email,
                        IsAvatarDefault = user.IsAvatarDefault, UserName = user.UserName,
                        IsEmailVerified = user.IsEmailVerified, RegistrationDate = user.RegistrationDate
                    });
            }
            else
            {
                users = context.Users.Select(user => new UserReturnModel
                    {
                        Id = user.Id, Roles = user.Roles, AvatarSource = user.AvatarSource, Email = user.Email,
                        IsAvatarDefault = user.IsAvatarDefault, UserName = user.UserName,
                        IsEmailVerified = user.IsEmailVerified, RegistrationDate = user.RegistrationDate
                    });
            }

            users = ApplySort(users, userSort);

            var usersReturnModel = new GetUsersModel
            {
                TotalUsersCount = await users.CountAsync(),
                Users = await users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync()
            };

            return usersReturnModel;
        }

        public async Task<GetUsersModel> GetUsersByRoleAsync(string? query, Guid roleId, UserSort userSort, int pageSize, int pageNumber)
        {
            IQueryable<UserReturnModel> users;
            if (query != null)
            {
                query = query.ToLower();
                users = context.Users
                    .Where(x => x.Roles.Any(y => y.Id == roleId))
                    .Where(x => x.UserName.ToLower().Contains(query) || x.Email.ToLower().Contains(query))
                    .Select(user => new UserReturnModel
                    {
                        Id = user.Id, Roles = user.Roles, AvatarSource = user.AvatarSource, Email = user.Email,
                        IsAvatarDefault = user.IsAvatarDefault, IsEmailVerified = user.IsEmailVerified,
                        RegistrationDate = user.RegistrationDate, UserName = user.UserName
                    });
            }
            else
            {
                users = context.Users
                    .Where(x => x.Roles.Any(y => y.Id == roleId))
                    .Select(user => new UserReturnModel
                    {
                        Id = user.Id, Roles = user.Roles, AvatarSource = user.AvatarSource, Email = user.Email,
                        IsAvatarDefault = user.IsAvatarDefault, IsEmailVerified = user.IsEmailVerified,
                        RegistrationDate = user.RegistrationDate, UserName = user.UserName
                    });
            }

            users = ApplySort(users, userSort);

            var usersReturnModel = new GetUsersModel
            {
                TotalUsersCount = await users.CountAsync(),
                Users = await users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync()
            };

            return usersReturnModel;
        }

        public async Task<GetUsersModel> GetUsersByRoleAsync(string? query, List<Guid> roleIds, UserSort userSort, int pageSize, int pageNumber)
        {
            IQueryable<UserReturnModel> users;
            if (query != null)
            {
                query = query.ToLower();
                users = context.Users
                    .Where(x => x.Roles.Any(y => roleIds.Contains(y.Id)))
                    .Where(x => x.UserName.ToLower().Contains(query) || x.Email.ToLower().Contains(query))
                    .Select(user => new UserReturnModel
                    {
                        Id = user.Id, Roles = user.Roles, AvatarSource = user.AvatarSource, Email = user.Email,
                        IsAvatarDefault = user.IsAvatarDefault, IsEmailVerified = user.IsEmailVerified,
                        RegistrationDate = user.RegistrationDate, UserName = user.UserName
                    });
            }
            else
            {
                users = context.Users
                    .Where(x => x.Roles.Any(y => roleIds.Contains(y.Id)))
                    .Select(user => new UserReturnModel
                    {
                        Id = user.Id, Roles = user.Roles, AvatarSource = user.AvatarSource, Email = user.Email,
                        IsAvatarDefault = user.IsAvatarDefault, IsEmailVerified = user.IsEmailVerified,
                        RegistrationDate = user.RegistrationDate, UserName = user.UserName
                    });
            }

            users = ApplySort(users, userSort);

            var usersReturnModel = new GetUsersModel
            {
                TotalUsersCount = await users.CountAsync(),
                Users = await users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync()
            };

            return usersReturnModel;
        }

        public async Task AddUserAsync(User user)
        {
            await context.Users.AddAsync(user);
        }

        public void UpdateUser(User user)
        {
            context.Users.Update(user);
        }

        private IQueryable<UserReturnModel> ApplySort(IQueryable<UserReturnModel> users, UserSort userSort)
        {
            switch (userSort)
            {
                case UserSort.None:
                    users = users.OrderBy(x => x.Id);
                    break;
                case UserSort.UserNameAsc:
                    users = users.OrderBy(x => x.UserName).ThenBy(x => x.Id);
                    break;
                case UserSort.UserNameDesc:
                    users = users.OrderByDescending(x => x.UserName).ThenBy(x => x.Id);
                    break;
                case UserSort.EmailAsc:
                    users = users.OrderBy(x => x.Email).ThenBy(x => x.Id);
                    break;
                case UserSort.EmailDesc:
                    users = users.OrderByDescending(x => x.Email).ThenBy(x => x.Id);
                    break;
                case UserSort.RegistrationDateAsc:
                    users = users.OrderBy(x => x.RegistrationDate).ThenBy(x => x.Id);
                    break;
                case UserSort.RegistrationDateDesc:
                    users = users.OrderByDescending(x => x.RegistrationDate).ThenBy(x => x.Id);
                    break;
            }

            return users;
        }
    }
}
