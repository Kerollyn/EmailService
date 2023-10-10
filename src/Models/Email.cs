namespace EmailService.Models
{
  public class Email
  {
    public int Id { get; set; }
    public string mailTo { get; set; }
    public string cc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string EmailType { get; set; }
    public string Caller { get; set; }
    public Guid emailuiid { get; set; }
    public string Status => EmailStatus.ToString();
    private EmailStatus EmailStatus { get; set; }

    public void SetStatus(EmailStatus emailStatus)
    {
      EmailStatus = emailStatus;
    }
  }
}
