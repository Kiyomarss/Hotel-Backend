using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Services;

public class DeleteBookingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DeleteBookingWorker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var deleteBookingConsumer = scope.ServiceProvider.GetRequiredService<DeleteBookingConsumer>();
                deleteBookingConsumer.ReceiveMessagesFromQueue("DeleteBookingQueue");
            }
        }, stoppingToken);
    }
}