namespace AccountMicroservice.Api.DTOs.User
{
    public class SetUserAvatarDto
    {
        public Guid UserId { get; set; }
        public byte[] AvatarSource { get; set; }
    }
}
