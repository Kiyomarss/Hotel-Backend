using Hotel_Core.RabbitMQ;

namespace Services;

using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class RabbitMqConsumerWorker : BackgroundService
{
    private readonly RabbitMqConsumer _rabbitMqConsumer;

    public RabbitMqConsumerWorker()
    {
        _rabbitMqConsumer = new RabbitMqConsumer();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => _rabbitMqConsumer.ReceiveMessagesFromQueue(), stoppingToken);
    }
}
