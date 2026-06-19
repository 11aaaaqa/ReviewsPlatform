namespace ReviewMicroservice.Api.Models.Business.Comments
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }
        public Guid? ReplyCommentId { get; set; }
        public Guid ReviewId { get; set; }

        public CommentStatus CommentStatus { get; set; }
        public string? RejectionReason { get; set; }
        public Guid? ConsideredByUserId { get; set; }
    }
}
