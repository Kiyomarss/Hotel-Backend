using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;

namespace Hotel_Core.RabbitMQ;

public class RabbitMqProducer
{
    public void SendMessageToQueue(string message, string queueName)
    {
        var factory = new ConnectionFactory()
        {
            HostName = Constant.Constant.RabbitMq.Hostname
        };

        try
        {
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // اطمینان از اینکه صف در صورت وجود ندارد، ایجاد شود
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to RabbitMQ: {ex.Message}");
        }
    }
}