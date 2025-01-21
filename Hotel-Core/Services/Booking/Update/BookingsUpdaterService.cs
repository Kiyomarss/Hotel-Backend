using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
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
    public class BookingsUpdaterService : IBookingsUpdaterService, IDisposable
    {
        private readonly RabbitMqProducer _rabbitMqProducer;
        private readonly ILogger<BookingsUpdaterService> _logger;
        private readonly IModel _persistentChannel;
        private readonly Subject<BookingResponse> _responseSubject = new();

        public BookingsUpdaterService(RabbitMqProducer rabbitMqProducer, ILogger<BookingsUpdaterService> logger)
        {
            _rabbitMqProducer = rabbitMqProducer;
            _logger = logger;

            // ایجاد کانال پایدار
            var connection = _rabbitMqProducer.CreateConnection();
            _persistentChannel = connection.CreateModel();

            SetupConsumer();
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
                var response = await WaitForResponseAsync(properties.CorrelationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending update message for booking ID {bookingId}: {ex.Message}");
                throw;
            }
        }

        private void SetupConsumer()
        {
            var consumer = new EventingBasicConsumer(_persistentChannel);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var bookingResponse = JsonConvert.DeserializeObject<BookingResponse>(message);

                    // بررسی و فراخوانی متد
                    if (bookingResponse != null)
                    {
                        bookingResponse.CorrelationId = ea.BasicProperties.CorrelationId; // مقداردهی CorrelationId
                        OnMessageReceived(bookingResponse);
                    }

                    _persistentChannel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing received message: {ex.Message}");
                }
            };

            _persistentChannel.BasicConsume(queue: "ResponseQueue", autoAck: false, consumer: consumer);
        }

        private async Task<BookingResponse> WaitForResponseAsync(string correlationId)
        {
            var task = _responseSubject.FirstAsync(r => r.CorrelationId == correlationId).ToTask();

            var timeout = Task.Delay(10000);
            var completedTask = await Task.WhenAny(task, timeout);

            if (completedTask == timeout)
            {
                throw new TimeoutException("No response received within the allowed time.");
            }

            return await task;
        }

        private void OnMessageReceived(BookingResponse bookingResponse)
        {
            _responseSubject.OnNext(bookingResponse);
        }

        public void Dispose()
        {
            _persistentChannel.Dispose();
            _responseSubject.Dispose();
        }
    }
}