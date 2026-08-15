using Web.MVC.Models.Api_responses.comment;

namespace Web.MVC.Models.View_models.Admin
{
    public class GetCommentsViewModel
    {
        public List<CommentResponse> Comments { get; set; }
        public bool IsNextPageExisted { get; set; }
        public int CurrentPageNumber { get; set; }
    }
}
