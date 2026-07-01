namespace Web.MVC.Models.Api_responses.comment
{
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
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
