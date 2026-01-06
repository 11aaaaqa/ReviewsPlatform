using SkiaSharp;
using Web.MVC.DTOs.account;

namespace Web.MVC.Services.User_services.Avatar_services
{
    public class AvatarService : IAvatarService
    {
        private readonly SKColor[] colors = new[]
        {
            SKColors.DeepSkyBlue, SKColors.Green, SKColors.CornflowerBlue, SKColors.Orange,
            SKColors.Coral, SKColors.MediumOrchid, SKColors.Brown, SKColors.DodgerBlue,
            SKColors.DarkMagenta, SKColors.Red, SKColors.DeepPink
        };

        public byte[] GetDefaultUserAvatar(RegisterDto registerModel, int size = 200)
        {
            string displaySymbol = registerModel.UserName.Trim().ToUpper()[0].ToString();

            int colorIndex = Math.Abs(registerModel.GetHashCode()) % (colors.Length - 1);
            SKColor backgroundColor = colors[colorIndex];

            using SKSurface surface = SKSurface.Create(new SKImageInfo(size, size));
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            using var paint = new SKPaint { Color = backgroundColor, IsAntialias = true };
            canvas.DrawCircle(size / 2, size / 2, size / 2, paint);

            using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true, };

            var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), size * 0.5f);
            var textBounds = new SKRect();
            font.MeasureText(displaySymbol, out textBounds);
            float y = size / 2 - textBounds.MidY;
            float x = size / 2 - textBounds.MidX;

            canvas.DrawText(displaySymbol, x, y, font, textPaint);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
