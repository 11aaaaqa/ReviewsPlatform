using AccountMicroservice.Api.Services.UserServices;
using AccountMicroservice.Api.Services.UserServices.EmailTokenServices;
using AccountMicroservice.Api.Services.UserServices.RoleServices;

namespace AccountMicroservice.Api.Services.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public IUserService UserService { get; }
        public IUserRolesService UserRolesService { get; }
        public IUserEmailTokenRepository UserEmailTokenRepository { get; }

        Task CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
