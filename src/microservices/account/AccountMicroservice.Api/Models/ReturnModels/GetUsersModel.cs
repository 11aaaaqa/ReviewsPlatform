namespace AccountMicroservice.Api.Models.ReturnModels
{
    public class GetUsersModel
    {
        public List<UserReturnModel> Users { get; set; }
        public int TotalUsersCount { get; set; }
    }
}
