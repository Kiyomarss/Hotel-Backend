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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var deleteBookingConsumer = scope.ServiceProvider.GetRequiredService<DeleteBookingConsumer>();
        
        await deleteBookingConsumer.ReceiveMessagesFromQueue("DeleteBookingQueue", stoppingToken);
    }
}
