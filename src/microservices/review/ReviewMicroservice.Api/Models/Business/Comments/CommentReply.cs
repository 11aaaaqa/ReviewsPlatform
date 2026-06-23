namespace ReviewMicroservice.Api.Models.Business.Comments
{
    public class CommentReply
    {
        public Guid ParentId { get; set; }
        public Guid RepliedId { get; set; }
    }
}
