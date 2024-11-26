using System.Net;
using System.Net.Mail;

namespace api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IKeyVaultService _keyVaultService;

    public EmailService(IConfiguration configuration, IKeyVaultService keyVaultService)
    {
        _configuration = configuration;
        _keyVaultService = keyVaultService;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = await _keyVaultService.GetSecretAsync("SmtpHost") ?? throw new InvalidOperationException("SMTP Host not configured");
        var portStr = await _keyVaultService.GetSecretAsync("SmtpPort") ?? throw new InvalidOperationException("SMTP Port not configured");
        var username = await _keyVaultService.GetSecretAsync("SmtpUsername") ?? throw new InvalidOperationException("SMTP Username not configured");
        var password = await _keyVaultService.GetSecretAsync("SmtpPassword") ?? throw new InvalidOperationException("SMTP Password not configured");
        var fromEmail = await _keyVaultService.GetSecretAsync("SmtpFromEmail") ?? throw new InvalidOperationException("SMTP From Email not configured");

        var port = int.Parse(portStr);

        var client = new SmtpClient(host)
        {
            Port = port,
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }
}