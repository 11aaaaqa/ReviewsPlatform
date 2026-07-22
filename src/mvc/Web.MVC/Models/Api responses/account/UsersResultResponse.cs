namespace Web.MVC.Models.Api_responses.account
{
    public class UsersResultResponse
    {
        public List<UserResponse> Users { get; set; }
        public int TotalUsersCount { get; set; }
        public bool IsNextPageExisted { get; set; }
    }
}
