using System.Net;
using System.Net.Mail;

namespace api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var client = new SmtpClient(smtpSettings["Host"])
        {
            Port = int.Parse(smtpSettings["Port"]!),
            Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpSettings["FromEmail"]!),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }
}