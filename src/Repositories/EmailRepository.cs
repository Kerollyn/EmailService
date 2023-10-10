using System.Text;
using Dapper;
using EmailService.Databases;
using EmailService.Extension;
using EmailService.Interfaces;
using EmailService.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace EmailService.Repository
{
  public class EmailRepository : IEmailRepository
  {
    private readonly IConfiguration _configuration;
    private readonly IDatabase _database;
    private readonly ILogger<EmailRepository> _logger;

    public EmailRepository(IConfiguration configuration,
                           IDatabase database,
                           ILogger<EmailRepository> logger)
    {
      _configuration = configuration;
      _database = database;
      _logger = logger;
    }

    public async Task CreateAsync(Email email)
    {
      using (var cn = _database.GetDbConnection())
      {
        var sql = new StringBuilder();
        sql.Append(" INSERT INTO Email(mailTo, cc, Subject, Body, EmailType, Caller, emailuiid, Status) ");
        sql.Append(" VALUES(@mailTo, @cc, @Subject, @Body, @EmailType, @Caller, @emailuiid,@Status) ");

        email.Id = await cn.InsertPostgresAsync(sql.ToString(), new
        {
          mailTo = email.mailTo,
          cc = email.cc,
          Subject = email.Subject,
          Body = email.Body,
          EmailType = email.EmailType,
          Caller = email.Caller,
          emailuiid = email.emailuiid,
          Status = email.Status
        });
      }
    }

    // public async Task<Email> GetAsync()
    // {
    //   using (var cn = _database.GetDbConnection())
    //   {
    //     var sql = new StringBuilder();
    //     sql.Append(" SELECT * FROM email ");

    //     return cn.QueryFirstOrDefault<Email>(sql.ToString());
    //   }
    // }

    public bool SendEmailAsync(string para, string assunto, string htmlBody)
    {
      try
      {
        var request = new MimeMessage();

        request.From.Add(MailboxAddress.Parse(_configuration.GetSection("User").Value));
        request.To.Add(MailboxAddress.Parse(para));
        request.Subject = assunto;
        request.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        using (var smtp = new SmtpClient())
        {
          smtp.Connect(_configuration.GetSection("Host").Value, 2525, SecureSocketOptions.StartTls);
          smtp.Authenticate(_configuration.GetSection("User").Value, _configuration.GetSection("Password").Value);
          smtp.Send(request);
          smtp.Disconnect(true);
        }

        _logger.LogInformation("Send to email");
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError($"Erro send to email: {ex.Message}");
        return false;
      }
    }


  }
}
