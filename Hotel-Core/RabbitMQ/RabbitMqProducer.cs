using RabbitMQ.Client;
using System.Text;

namespace Hotel_Core.RabbitMQ;

public class RabbitMqProducer
{
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMqProducer()
    {
        _connectionFactory = new ConnectionFactory()
        {
            HostName = Constant.Constant.RabbitMq.Hostname
        };
    }

    // متد برای ایجاد اتصال
    public IConnection CreateConnection()
    {
        return _connectionFactory.CreateConnection();
    }

    // متد برای ایجاد BasicProperties
    public IBasicProperties CreateBasicProperties(IModel channel)
    {
        return channel.CreateBasicProperties();
    }

    public void SendMessageToQueue(string message, string queueName)
    {
        try
        {
            using var connection = CreateConnection();
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to RabbitMQ: {ex.Message}");
        }
    }
    
    public void SendMessageToQueueWithProperties(string message, string queueName, IBasicProperties properties)
    {
        try
        {
            using var connection = CreateConnection();
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
                                 basicProperties: properties,
                                 body: body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to RabbitMQ: {ex.Message}");
        }
    }

    
    public void SendMessageWithReplyQueue(string message, string queueName, string replyQueueName, string correlationId)
    {
        using var connection = CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var properties = CreateBasicProperties(channel);
        properties.CorrelationId = correlationId;
        properties.ReplyTo = replyQueueName;

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: properties,
                             body: body);

        Console.WriteLine($"Message sent to queue '{queueName}' with CorrelationId: {correlationId}");
    }
}