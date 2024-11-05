using Core.Models;

namespace Core.Interfaces;

public interface IEmailService
{
    Task SendEmail(EmailDto email);
}