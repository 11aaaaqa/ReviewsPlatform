namespace Web.MVC.Models.View_models.User
{
    public class EditUserProfileViewModel
    {
        public Guid UserId { get; set; }
        public bool IsAvatarDefault { get; set; }
        public string AvatarSrc { get; set; }
        public string UserEmail { get; set; }
    }
}
