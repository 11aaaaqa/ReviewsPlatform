using AccountMicroservice.Api.Models.Business;

namespace AccountMicroservice.Api.Models.ReturnModels
{
    public class UserReturnModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public byte[] AvatarSource { get; set; }
        public bool IsAvatarDefault { get; set; }
        public DateOnly RegistrationDate { get; set; }
        public List<Role> Roles { get; set; } = new();
    }
}
