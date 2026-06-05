namespace CategoryMicroservice.Api.Services
{
    public class ImageValidator
    {
        public bool IsImage(byte[] source)
        {
            if(source.Length < 8)
                return false;

            byte[] png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x01 };
            byte[] jpg = { 0xFF, 0xD8, 0xFF };

            if(source.Take(8).SequenceEqual(png)) return true;
            if (source.Take(3).SequenceEqual(jpg)) return true;

            return false;
        }
    }
}
