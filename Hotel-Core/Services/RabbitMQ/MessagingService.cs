using RabbitMQ.Client;
using System.Text;

namespace Hotel_Core.Services.RabbitMQ
{
    public class MessagingService
    {
        private readonly string _hostname = "localhost"; // یا آدرس RabbitMQ
        private readonly string _queueName = "testQueue";

        public void SendMessageToQueue(string message)
        {
            var factory = new ConnectionFactory() { HostName = _hostname };

            // استفاده از متد همزمان CreateConnection
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) // استفاده از CreateModel
            {
                // اعلام صف در صورت عدم وجود آن
                channel.QueueDeclare(queue: _queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // تبدیل پیام به بایت‌های UTF-8
                var body = Encoding.UTF8.GetBytes(message);

                // ارسال پیام به صف
                channel.BasicPublish(exchange: "",
                                     routingKey: _queueName,
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}