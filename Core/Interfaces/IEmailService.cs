using Core.Models;

namespace Core.Interfaces;

public interface IEmailService
{
    void SendEmail(EmailDto email);
}