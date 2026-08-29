using Web.MVC.Models.Api_responses.account;
using Web.MVC.Models.Api_responses.restriction;

namespace Web.MVC.Models.View_models.User
{
    public class GetUserByIdViewModel
    {
        public UserResponse User { get; set; }
        public Guid? CurrentUserId { get; set; } = null;
        public bool CanUserRejectReviews { get; set; }
        public bool CanUserRejectComments { get; set; }
        public bool CanUserSetTheRoles { get; set; }
        public bool CanUserViewTheRoles { get; set; }
        public bool CanUserViewCommsReviewsWithDifferentStatuses { get; set; }
        public string AvatarSrc { get; set; }
        public RestrictionResponse? ActiveRestriction { get; set; }
        public List<RoleResponse> AllRoles { get; set; } = new();
    }
}
