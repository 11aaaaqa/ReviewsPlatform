namespace Web.MVC.Models.Api_responses.comment
{
    public class CommentsResultResponse
    {
        public List<CommentResponse> Comments { get; set; }
        public bool IsNextPageExisted { get; set; }
    }
}
