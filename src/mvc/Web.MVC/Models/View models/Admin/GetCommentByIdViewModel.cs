using Web.MVC.Models.Api_responses.comment;
using Web.MVC.Models.Api_responses.review;
using Web.MVC.Models.View_models.Category;
using Web.MVC.Models.View_models.User;

namespace Web.MVC.Models.View_models.Admin
{
    public class GetCommentByIdViewModel
    {
        public CommentResponse Comment { get; set; }
        public UserDisplay UserCommentCreatedBy { get; set; }
        public ItemDisplay Item { get; set; }
        public ReviewResponse Review { get; set; }
        public bool IsUserRestricted { get; set; }
    }
}
