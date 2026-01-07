using Web.MVC.DTOs.account;

namespace Web.MVC.Services.User_services.Avatar_services
{
    public interface IAvatarService
    {
        byte[] GetDefaultUserAvatar(RegisterDto registerModel, int size = 200);
    }
}
