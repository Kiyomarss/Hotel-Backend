using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;

namespace Hotel_Core.RabbitMQ;

public class RabbitMqProducer
{
    private readonly RabbitMqOptions _options;

    public RabbitMqProducer(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public void SendMessageToQueue(string message, string queueName)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _options.Hostname
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);

        Console.WriteLine($"Message sent to queue '{queueName}': {message}");
    }
}