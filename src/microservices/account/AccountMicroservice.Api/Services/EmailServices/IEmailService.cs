namespace AccountMicroservice.Api.Services.EmailServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toAddress, string title, string text);
    }
}
