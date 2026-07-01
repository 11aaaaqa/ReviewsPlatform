using ReviewMicroservice.Api.Models.Business.Comments;

namespace ReviewMicroservice.Api.DTOs
{
    public class CommentsResult
    {
        public List<Comment> Comments { get; set; }
        public bool IsNextPageExisted { get; set; }
    }
}
