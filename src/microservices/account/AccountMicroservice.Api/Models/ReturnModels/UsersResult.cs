namespace AccountMicroservice.Api.Models.ReturnModels
{
    public class UsersResult
    {
        public List<UserReturnModel> Users { get; set; }
        public int TotalUsersCount { get; set; }
        public bool IsNextPageExisted { get; set; }
    }
}
