using Web.MVC.Models.Api_responses.comment;
using Web.MVC.Models.View_models.User;

namespace Web.MVC.Models.View_models.Comment
{
    public class CommentDisplay
    {
        public Guid Id { get; set; }
        public UserDisplay User { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }
        public int RepliesCount { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid ReviewId { get; set; }

        public CommentStatus CommentStatus { get; set; }
        public string? RejectionReason { get; set; }
        public Guid? ConsideredByUserId { get; set; }
    }
}
