using EmailService.Models;

namespace EmailService.Interfaces
{
  public interface IEmailRepository
  {
    //void SendEmail(Email email);

    bool SendEmailAsync(string mailTo, string subject, string htmlBody);
    Task CreateAsync(Email email);
    // Task<Email> GetAsync();
  }
}
