using Core.Interfaces;
using Core.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public void SendEmail(EmailDto request)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("noreply@trackin.life"));
        email.To.Add(MailboxAddress.Parse(request.To));
        email.Subject = request.Subject;
        email.Body = new TextPart(TextFormat.Html) { Text = request.Body };

        using var smtp = new SmtpClient();
        smtp.Connect("email-smtp.eu-north-1.amazonaws.com", 587, SecureSocketOptions.StartTls);
        smtp.Authenticate("AKIAVRUVV4IBUNPT46NY", "BAGoemwGRYj6ruIi0Kr6is2JopozXLsEBCMDBWuiZea3");
        smtp.Send(email);
        smtp.Disconnect(true);
    }
}