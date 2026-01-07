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

            int colorIndex = Math.Abs(registerModel.GetHashCode()) % colors.Length;
            SKColor backgroundColor = colors[colorIndex];

            using SKSurface surface = SKSurface.Create(new SKImageInfo(size, size));
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            using var paint = new SKPaint();
            paint.Color = backgroundColor;
            paint.IsAntialias = true;

            canvas.DrawCircle(size / 2f, size / 2f, size / 2f, paint);

            using var textPaint = new SKPaint();
            textPaint.Color = SKColors.White;
            textPaint.IsAntialias = true;

            using var font = new SKFont(SKTypeface.FromFamilyName("Noto Sans", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
                size * 0.5f);
            font.MeasureText(displaySymbol, out SKRect textBounds);

            float y = size / 2f - textBounds.MidY;
            canvas.DrawText(displaySymbol, new SKPoint(size / 2f, y), SKTextAlign.Center, font, textPaint);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
