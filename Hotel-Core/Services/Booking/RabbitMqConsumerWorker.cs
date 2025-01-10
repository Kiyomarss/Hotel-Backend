using Hotel_Core.RabbitMQ;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class RabbitMqConsumerWorker : BackgroundService
    {
        private readonly RabbitMqConsumer _rabbitMqConsumer;

        public RabbitMqConsumerWorker(RabbitMqConsumer rabbitMqConsumer)
        {
            _rabbitMqConsumer = rabbitMqConsumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // تعریف تابع پردازش پیام
            Action<string> processMessage = message =>
            {
                Console.WriteLine($"Processed message: {message}");
                // اینجا می‌توانید منطق پردازش پیام خود را اضافه کنید
            };

            // ارسال تابع پردازش به متد
            return Task.Run(() => _rabbitMqConsumer.ReceiveMessagesFromQueue(processMessage), stoppingToken);
        }
    }
}