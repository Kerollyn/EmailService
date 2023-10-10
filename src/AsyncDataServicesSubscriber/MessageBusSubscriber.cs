using System.Text;
using System.Text.Json;
using EmailService.Interfaces;
using EmailService.Models;
using EmailService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailService.AsyncDataServicesSubscriber
{
  public class MessageBusSubscriber : BackgroundService
  {
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IModel _channel;
    private readonly ILogger<MessageBusSubscriber> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MessageBusSubscriber(IConfiguration configuration,
                                IServiceScopeFactory scopeFactory,
                                ILogger<MessageBusSubscriber> logger)
    {
      _configuration = configuration;
      _scopeFactory = scopeFactory;
      _logger = logger;

      ConnectRabbitMq();
    }

    private void ConnectRabbitMq()
    {
      var factory = new ConnectionFactory()
      {
        HostName = _configuration["RabbitMQHost"],
        Port = int.Parse(_configuration["RabbitMQPort"])
      };
      _connection = factory.CreateConnection();
      _channel = _connection.CreateModel();

      Console.WriteLine("--> Espera o message bus...");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var consumer = InitializerConsumer(_channel, nameof(Email));

      consumer.Received += (model, ea) =>
      {
        var receiveMessage = Encoding.UTF8.GetString(ea.Body.ToArray());
        var email = JsonSerializer.Deserialize<Email>(receiveMessage);
        try
        {
          using (var scope = _scopeFactory.CreateScope())
          {
            var repository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
            try
            {
              repository.SendEmailAsync(email.mailTo, email.Subject, email.Body);
              email.SetStatus(ProcessEmailStatus(true));

              repository.CreateAsync(email);
            }
            catch (Exception ex)
            {
              _logger.LogError($"Log Erro => {ex.Message}");
            }
          };

          var emailsend = new
          {
            email.Caller,
            email.emailuiid,
            email.Status
          };

          var replyMessage = JsonSerializer.Serialize(emailsend);
          SendReplyMessage(replyMessage, _channel, ea);
          Console.WriteLine("--> replyMessage ...");

          _logger.LogInformation($"Email send successful.");
          _logger.LogInformation($"Reply message: {replyMessage}");
        }
        catch (Exception ex)
        {
          email.SetStatus(ProcessEmailStatus(false));
          _logger.LogError($"Erro send to email: {ex.Message}");
        }
      };

      // while (!stoppingToken.IsCancellationRequested)
      // {
      //   _logger.LogInformation("Serviço rodando: {time}", DateTimeOffset.Now);
      //   await Task.Delay(10000, stoppingToken);
      // }
      // await Task.CompletedTask;
      Console.ReadLine();
    }

    private void SendReplyMessage(string replyMessage, IModel channel, BasicDeliverEventArgs ea)
    {
      try
      {
        var props = ea.BasicProperties;
        var replyProps = channel.CreateBasicProperties();

        var responseBytes = Encoding.UTF8.GetBytes(replyMessage);

        channel.BasicPublish(exchange: "",
                             routingKey: "Email_Return",
                             basicProperties: replyProps,
                             body: responseBytes);

        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                         multiple: false);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Erro reply message: {ex.Message}");
      }
    }

    private static EventingBasicConsumer InitializerConsumer(IModel channel, string queueName)
    {
      channel.QueueDeclare(queue: "Email",
                           durable: true,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);

      channel.BasicQos(0, 1, false);

      var consumer = new EventingBasicConsumer(channel);
      channel.BasicConsume(queue: "Email",
                           autoAck: false,
                           consumer: consumer);

      return consumer;
    }

    private static EmailStatus ProcessEmailStatus(bool test)
    {
      return EmailServiceTest.OnStore(test);
    }
  }
}

