namespace Web.MVC.Services
{
    public class ImageConverter
    {
        public string GetImageSrc(byte[] imageSource)
            => $"data:image/png;base64,{Convert.ToBase64String(imageSource)}";
    }
}
