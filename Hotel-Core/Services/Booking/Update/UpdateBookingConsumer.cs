using Hotel_Core.ServiceContracts;
using RepositoryContracts;
using System.Text;
using Hotel_Core.Constant;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Services;

public class UpdateBookingConsumer : IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public UpdateBookingConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        var factory = new ConnectionFactory()
        {
            HostName = Constant.RabbitMq.Hostname
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public async Task ReceiveMessagesFromQueue(string queueName, CancellationToken stoppingToken)
    {
        Console.WriteLine($"Connecting1111 to queue: {queueName}");

        _channel.QueueDeclare(
                              queue: queueName,
                              durable: true,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);


        _channel.QueueDeclare(
                              queue: "ResponseQueue",
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

            try
            {
                // ایجاد دامنه جدید برای هر پیام
                using var scope = _scopeFactory.CreateScope();
                var bookingsRepository = scope.ServiceProvider.GetRequiredService<IBookingsRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var updateMessage = JsonConvert.DeserializeObject<dynamic>(messageJson);
                Guid bookingId = updateMessage.BookingId;
                var patchDoc = JsonConvert.DeserializeObject<JsonPatchDocument<Booking>>(updateMessage.PatchDocument.ToString());

                var matchingBooking = await bookingsRepository.GetBookingByBookingId(bookingId);

                if (matchingBooking == null)
                    throw new ArgumentException("Booking ID does not exist");

                patchDoc.ApplyTo(matchingBooking);

                if (matchingBooking.StartDate > matchingBooking.EndDate)
                    throw new InvalidOperationException("Invalid booking data");

                await unitOfWork.BeginTransactionAsync();
                try
                {
                    var updatedBooking = await bookingsRepository.UpdateBooking(matchingBooking);
                    await unitOfWork.SaveChangesAsync();
                    await unitOfWork.CommitTransactionAsync();

                    var responseMessage = JsonConvert.SerializeObject(updatedBooking.ToBookingResponse());
                    var responseProperties = _channel.CreateBasicProperties();
                    responseProperties.CorrelationId = ea.BasicProperties.CorrelationId;

                    _channel.BasicPublish(
                                          exchange: "",
                                          routingKey: "ResponseQueue",
                                          basicProperties: responseProperties,
                                          body: Encoding.UTF8.GetBytes(responseMessage));

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    await unitOfWork.RollbackTransactionAsync();

                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing update message: {ex.Message}");
            }
        };

        _channel.BasicConsume(
                              queue: queueName,
                              autoAck: false,
                              consumer: consumer);

        Console.WriteLine($"Listening for {queueName}...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}