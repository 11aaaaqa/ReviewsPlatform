namespace AccountMicroservice.Api.Models.General
{
    public class FormatHashResult
    {
        public byte[] PasswordHash { get; set; }
        public byte[] Salt { get; set; }
    }
}
