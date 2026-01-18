namespace Web.MVC.Services
{
    public class AvatarConverter
    {
        public string GetAvatarSrc(byte[] avatarSource)
            => $"data:image/png;base64,{Convert.ToBase64String(avatarSource)}";
    }
}
