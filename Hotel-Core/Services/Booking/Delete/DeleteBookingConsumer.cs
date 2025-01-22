using Hotel_Core.ServiceContracts;
using RepositoryContracts;
using System.Text;
using Hotel_Core.Constant;
using Hotel_Core.Services.Base;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Services;

public class DeleteBookingConsumer : DisposableBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public DeleteBookingConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        var factory = new ConnectionFactory();

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public async Task ReceiveMessagesFromQueue(string queueName, CancellationToken stoppingToken)
    {
        _channel.QueueDeclare(queue: queueName,
                              durable: true,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            if (stoppingToken.IsCancellationRequested)
            {
                _channel.BasicAck(ea.DeliveryTag, multiple: false);

                return;
            }

            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);

            IUnitOfWork unitOfWork = null!;
            bool transactionStarted = false;

            try
            {
                var deleteMessage = JsonConvert.DeserializeObject<dynamic>(messageJson);
                Guid bookingId = deleteMessage.BookingId;

                using var scope = _scopeFactory.CreateScope();

                var bookingsRepository = scope.ServiceProvider.GetRequiredService<IBookingsRepository>();
                unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // Start the transaction
                await unitOfWork.BeginTransactionAsync();
                transactionStarted = true;

                var booking = await bookingsRepository.FindBookingById(bookingId);
                if (booking == null)
                {
                    throw new KeyNotFoundException($"Booking with ID {bookingId} does not exist.");
                }

                await bookingsRepository.DeleteBooking(bookingId);
                await unitOfWork.CommitTransactionAsync();

                // Acknowledge the message after successful processing
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
                Console.WriteLine($"Booking with ID {bookingId} deleted successfully.");
            }
            catch (Exception ex)
            {
                if (transactionStarted)
                {
                    await unitOfWork.RollbackTransactionAsync();
                }

                Console.WriteLine($"Error while deleting booking: {ex.Message}");
            }
        };

        _channel.BasicConsume(queue: queueName,
                              autoAck: false,
                              consumer: consumer);

        Console.WriteLine($"Listening for {queueName}...");

        // Wait for cancellation
        while (!stoppingToken.IsCancellationRequested)
        {
            // Allow the task to process more messages
            await Task.Delay(1000, stoppingToken);
        }
    }

    protected override void DisposeManagedResources()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}