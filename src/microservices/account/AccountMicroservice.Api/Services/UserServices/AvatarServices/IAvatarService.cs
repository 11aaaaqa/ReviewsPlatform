using AccountMicroservice.Api.Models.Business;

namespace AccountMicroservice.Api.Services.UserServices.AvatarServices
{
    public interface IAvatarService
    {
        byte[] GetDefaultUserAvatar(User user, int size = 200);
    }
}
