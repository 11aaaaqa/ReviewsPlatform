using Web.MVC.Models.View_models.Review;

namespace Web.MVC.Models.View_models.Admin
{
    public class GetReviewByIdAdminViewModel
    {
        public ReviewDisplay Review { get; set; }
        public bool IsUserRestricted { get; set; }
    }
}
