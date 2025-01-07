using Hotel_Core.ServiceContracts;
using RepositoryContracts;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace Services;

public class DeleteBookingConsumer
{
    private readonly IBookingsRepository _bookingsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBookingConsumer(IBookingsRepository bookingsRepository, IUnitOfWork unitOfWork)
    {
        _bookingsRepository = bookingsRepository;
        _unitOfWork = unitOfWork;
    }

    public void ReceiveMessagesFromQueue(string queueName)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                try
                {
                    // دریافت پیام و تبدیل به شیء
                    var deleteMessage = JsonConvert.DeserializeObject<dynamic>(messageJson);
                    Guid bookingId = deleteMessage.BookingId;

                    // اجرای تراکنش حذف
                    await _unitOfWork.BeginTransactionAsync();
                    var booking = await _bookingsRepository.FindBookingById(bookingId);
                    if (booking == null)
                    {
                        throw new KeyNotFoundException($"Booking with ID {bookingId} does not exist.");
                    }

                    await _bookingsRepository.DeleteBooking(bookingId);
                    await _unitOfWork.CommitTransactionAsync();

                    Console.WriteLine($"Booking with ID {bookingId} deleted successfully.");
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    Console.WriteLine($"Error while deleting booking: {ex.Message}");
                }
            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine($"Listening for {queueName}...");
            Console.ReadLine();
        }
    }
}
