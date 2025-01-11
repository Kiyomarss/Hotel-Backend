using Hotel_Core.ServiceContracts;
using RepositoryContracts;
using System.Text;
using Hotel_Core.Services.Base;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Services;

public class DeleteBookingConsumer : DisposableBase
{
    private readonly IBookingsRepository _bookingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public DeleteBookingConsumer(IBookingsRepository bookingsRepository, IUnitOfWork unitOfWork)
    {
        _bookingsRepository = bookingsRepository;
        _unitOfWork = unitOfWork;

        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

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

            bool transactionStarted = false;

            try
            {
                var deleteMessage = JsonConvert.DeserializeObject<dynamic>(messageJson);
                Guid bookingId = deleteMessage.BookingId;

                // Start the transaction
                await _unitOfWork.BeginTransactionAsync();
                transactionStarted = true;

                var booking = await _bookingsRepository.FindBookingById(bookingId);
                if (booking == null)
                {
                    throw new KeyNotFoundException($"Booking with ID {bookingId} does not exist.");
                }

                await _bookingsRepository.DeleteBooking(bookingId);
                await _unitOfWork.CommitTransactionAsync();

                // Acknowledge the message after successful processing
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
                Console.WriteLine($"Booking with ID {bookingId} deleted successfully.");
            }
            catch (Exception ex)
            {
                if (transactionStarted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
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