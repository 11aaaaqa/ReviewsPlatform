using AccountMicroservice.Api.Models.Business;
using SkiaSharp;

namespace AccountMicroservice.Api.Services.UserServices.AvatarServices
{
    public class AvatarService : IAvatarService
    {
        private readonly SKColor[] colors = new[]
        {
            SKColors.DeepSkyBlue, SKColors.Green, SKColors.CornflowerBlue, SKColors.Orange,
            SKColors.Coral, SKColors.MediumOrchid, SKColors.Brown, SKColors.DodgerBlue,
            SKColors.DarkMagenta, SKColors.Red, SKColors.DeepPink
        };

        public byte[] GetDefaultUserAvatar(User user, int size = 200)
        {
            string displaySymbol = user.UserName.Trim().ToUpper()[0].ToString();

            int colorIndex = Math.Abs(user.GetHashCode()) % colors.Length;
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

        public byte[] CropCustomUserAvatar(byte[] avatarSource)
        {
            using SKBitmap bitmap = SKBitmap.Decode(avatarSource);
            int size = Math.Min(bitmap.Height, bitmap.Width);
            using SKBitmap editedBitmap = new SKBitmap(size, size);

            using SKCanvas canvas = new SKCanvas(editedBitmap);
            using SKPath path = new SKPath();

            canvas.Clear(SKColors.Transparent);
            path.AddCircle(size / 2f, size / 2f, size / 2f);
            canvas.ClipPath(path);
            canvas.DrawBitmap(bitmap, (size - bitmap.Width) / 2f, (size - bitmap.Height) / 2f);

            using SKImage editedImage = SKImage.FromBitmap(editedBitmap);
            using SKData data = editedImage.Encode(SKEncodedImageFormat.Png, 100);

            return data.ToArray();
        }

        public bool ValidateAvatar(byte[] avatarSource)
        {
            using var memoryStream = new MemoryStream(avatarSource);
            using var codec = SKCodec.Create(memoryStream);
            var format = codec.EncodedFormat;
            if (format != SKEncodedImageFormat.Png && format != SKEncodedImageFormat.Jpeg)
                return false;

            return true;
        }
    }
}
