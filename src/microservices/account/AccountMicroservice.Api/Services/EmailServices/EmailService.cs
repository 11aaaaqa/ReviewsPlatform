using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace AccountMicroservice.Api.Services.EmailServices
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        public async Task SendEmailAsync(string toAddress, string title, string text)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(configuration["COMPANY_NAME"], configuration["EMAIL_ADDRESS"]));
            message.To.Add(new MailboxAddress(toAddress, toAddress));
            message.Subject = title;
            message.Body = new TextPart(TextFormat.Html) { Text = text };

            using var client = new SmtpClient();
            await client.ConnectAsync(configuration["EMAIL_HOST"], int.Parse(configuration["EMAIL_PORT"]!), true);
            await client.AuthenticateAsync(configuration["EMAIL_ADDRESS"], configuration["EMAIL_PASSWORD"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
