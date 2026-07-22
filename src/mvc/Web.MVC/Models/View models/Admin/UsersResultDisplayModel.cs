using Web.MVC.Models.View_models.User;

namespace Web.MVC.Models.View_models.Admin
{
    public class UsersResultDisplayModel
    {
        public List<UserFormattedDateDisplay> Users { get; set; }
        public int TotalUsersCount { get; set; }
        public bool IsNextPageExisted { get; set; }
    }
}
