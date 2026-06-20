using ReviewMicroservice.Api.Services.CommentServices;
using ReviewMicroservice.Api.Services.ReviewServices;
using ReviewMicroservice.Api.Services.ReviewServices.ReactionServices.Repository;

namespace ReviewMicroservice.Api.Services.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IReviewRepository ReviewRepository { get; }
        IReactionRepository ReactionRepository { get; }
        ICommentRepository CommentRepository { get; }

        Task CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
