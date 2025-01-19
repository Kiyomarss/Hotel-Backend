using System.Text;
using Hotel_Core.RabbitMQ;
using Newtonsoft.Json;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Microsoft.AspNetCore.JsonPatch;
using ServiceContracts;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Services
{
    public class BookingsUpdaterService : IBookingsUpdaterService
    {
        private readonly RabbitMqProducer _rabbitMqProducer;
        private readonly ILogger<BookingsUpdaterService> _logger;
        private readonly IModel _persistentChannel;

        public BookingsUpdaterService(RabbitMqProducer rabbitMqProducer, ILogger<BookingsUpdaterService> logger)
        {
            _rabbitMqProducer = rabbitMqProducer;
            _logger = logger;

            // ایجاد کانال پایدار
            var connection = _rabbitMqProducer.CreateConnection();
            _persistentChannel = connection.CreateModel();
        }

        public async Task<BookingResponse> InitiateUpdateBooking(Guid bookingId, JsonPatchDocument<Booking> patchDoc)
        {
            if (bookingId == Guid.Empty)
                throw new ArgumentException("Invalid booking ID");

            if (patchDoc == null)
                throw new ArgumentNullException(nameof(patchDoc));

            var updateMessage = new
            {
                BookingId = bookingId,
                PatchDocument = patchDoc
            };

            var messageJson = JsonConvert.SerializeObject(updateMessage);

            using var connection = _rabbitMqProducer.CreateConnection();
            using var channel = connection.CreateModel();

            var properties = _rabbitMqProducer.CreateBasicProperties(channel);
            properties.CorrelationId = Guid.NewGuid().ToString();

            try
            {
                // ارسال پیام
                _rabbitMqProducer.SendMessageToQueueWithProperties(messageJson, "UpdateBookingQueue", properties);

                // منتظر دریافت پاسخ از صف دائمی
                var response = await WaitForResponseAsync(properties.CorrelationId, channel);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending update message for booking ID {bookingId}: {ex.Message}");
                throw;
            }
        }

        public async Task<BookingResponse> WaitForResponseAsync(string correlationId, IModel channel)
        {
            if (!channel.IsOpen)
                throw new InvalidOperationException("Channel is already closed.");

            var consumer = new EventingBasicConsumer(channel);
            var tcs = new TaskCompletionSource<BookingResponse>();

            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var bookingResponse = JsonConvert.DeserializeObject<BookingResponse>(message);
                        
                        channel.BasicAck(ea.DeliveryTag, false);
                        tcs.TrySetResult(bookingResponse);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing response message: {ex.Message}");
                        tcs.TrySetException(ex);
                    }
                }
            };

            channel.BasicConsume(queue: "ResponseQueue", autoAck: false, consumer: consumer);

            var timeout = Task.Delay(10000);
            var completedTask = await Task.WhenAny(tcs.Task, timeout);

            if (completedTask == timeout)
            {
                Console.WriteLine("Timeout occurred while waiting for response.");

                throw new TimeoutException("No response received within the allowed time.");
            }

            return await tcs.Task;
        }
        
        public void Dispose()
        {
            _persistentChannel?.Dispose();
        }
    }
}