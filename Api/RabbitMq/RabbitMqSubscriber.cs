using Api.Dtos;
using LoginMicroservice.Api.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Api.RabbitMq;

public class RabbitMqSubscriber : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly string _queueName;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqSubscriber(IConfiguration configuration)
    {
        _configuration = configuration;
        _connection = new ConnectionFactory() { HostName = _configuration["RabbitMqHost"], Port = int.Parse(_configuration["RabbitMqPort"]), UserName = _configuration["RabbitMqUser"], Password = _configuration["RabbitMqPassword"] }.CreateConnection();
        _channel = _connection.CreateModel();
        _queueName = _channel.QueueDeclare(queue: _configuration["EmailQueueName"]).QueueName;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EventingBasicConsumer? consumer = new(_channel);

        consumer.Received += async (ModuleHandle, ea) =>
        {
            ReadOnlyMemory<byte> body = ea.Body;
            string? mensagem = Encoding.UTF8.GetString(body.ToArray());
            var dto = JsonSerializer.Deserialize<EmailDto>(mensagem) ?? throw new Exception("Error in deserialize DTO");
            EmailSender.SendEmail(dto, _configuration);
        };
        _channel.BasicConsume(_queueName, autoAck: true, consumer);
        return Task.CompletedTask;
    }
}
