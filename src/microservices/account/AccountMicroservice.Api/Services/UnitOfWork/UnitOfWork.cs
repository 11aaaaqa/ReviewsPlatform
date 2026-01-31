using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Exceptions;
using AccountMicroservice.Api.Services.UserServices;
using AccountMicroservice.Api.Services.UserServices.EmailTokenServices;
using AccountMicroservice.Api.Services.UserServices.RoleServices;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccountMicroservice.Api.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserService UserService { get; }
        public IUserRolesService UserRolesService { get; }
        public IUserEmailTokenRepository UserEmailTokenRepository { get; }
        private readonly ApplicationDbContext context;
        private IDbContextTransaction? transaction;


        public UnitOfWork(IUserEmailTokenRepository userEmailTokenRepository, IUserService userService,
            IUserRolesService userRolesService, ApplicationDbContext context)
        {
            UserService = userService;
            UserRolesService = userRolesService;
            UserEmailTokenRepository = userEmailTokenRepository;
            this.context = context;
        }

        public async Task CompleteAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (transaction != null)
                throw new TransactionAlreadyBeganException();
            transaction = await context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (transaction == null)
                throw new TransactionHasNotBegunException();

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            await transaction.DisposeAsync();
            transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (transaction == null)
                throw new TransactionHasNotBegunException();

            try
            {
                await transaction.RollbackAsync();
            }
            finally
            {
                await transaction.DisposeAsync();
                transaction = null;
            }
        }

        public void Dispose()
        {
            transaction?.Dispose();
            transaction = null;
        }
    }
}
