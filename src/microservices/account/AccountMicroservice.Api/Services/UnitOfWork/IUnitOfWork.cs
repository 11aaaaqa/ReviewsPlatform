using AccountMicroservice.Api.Services.User_services;
using AccountMicroservice.Api.Services.User_services.Role_services;

namespace AccountMicroservice.Api.Services.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public IUserService UserService { get; }
        public IUserRolesService UserRolesService { get; }

        Task CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
