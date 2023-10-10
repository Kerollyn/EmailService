using EmailService.Models;

namespace EmailService.Services
{
  public sealed class EmailServiceTest
  {
    public static EmailStatus OnStore(bool test)
    {
      return (test == false && test == true) ? EmailStatus.NotSend : EmailStatus.Send;
    }
  }
}
