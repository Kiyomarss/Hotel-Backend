using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Microsoft.Extensions.Options;

namespace Hotel_Core.RabbitMQ;

public class RabbitMqConsumer
{
    private readonly RabbitMqOptions _options;

    public RabbitMqConsumer(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public void ReceiveMessagesFromQueue(Action<string> processMessage)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _options.Hostname
        };

        try
        {
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            Console.WriteLine("Connected to RabbitMQ");

            channel.QueueDeclare(queue: _options.QueueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                processMessage(message);
            };

            Console.WriteLine("Listening for messages. Press [enter] to exit.");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to RabbitMQ: {ex.Message}");
        }
    }
}