using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Services;

public class UpdateBookingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpdateBookingWorker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<UpdateBookingConsumer>();

            try
            {
                // دریافت پیام‌ها از صف
                await consumer.ReceiveMessagesFromQueue("UpdateBookingQueue", stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateBookingWorker: {ex.Message}");
            }

            await Task.Delay(1000, stoppingToken); // جلوگیری از اجرای مداوم در صورت بروز خطا
        }
    }
}
