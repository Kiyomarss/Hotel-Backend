using RabbitMQ.Client;
using System.Text;

namespace Hotel_Core.RabbitMQ;

public class RabbitMqProducer
{
    private readonly string _hostname = "localhost";

    public void SendMessageToQueue(string message, string queueName)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // اطمینان از وجود صف مورد نظر
            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            // ارسال پیام به صف مشخص‌شده
            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine($"Message sent to queue '{queueName}': {message}");
        }
    }
}