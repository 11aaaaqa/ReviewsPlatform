using AccountMicroservice.Api.Models.Business;

namespace AccountMicroservice.Api.Services.User_services.Avatar_services
{
    public interface IAvatarService
    {
        byte[] GetDefaultUserAvatar(User user, int size = 200);
    }
}
