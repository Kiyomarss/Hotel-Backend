using Hotel_Core.RabbitMQ;
using Newtonsoft.Json;
using ServiceContracts;

namespace Services
{
    public class BookingsDeleterService : IBookingsDeleterService
    {
        private readonly RabbitMqProducer _rabbitMqProducer;

        public BookingsDeleterService(RabbitMqProducer rabbitMqProducer)
        {
            _rabbitMqProducer = rabbitMqProducer;
        }

        public Task<bool> InitiateDeleteBooking(Guid bookingId)
        {
            if (bookingId == Guid.Empty)
                throw new ArgumentException("Invalid booking ID");

            // ایجاد پیام
            var deleteMessage = new
            {
                BookingId = bookingId
            };

            // ارسال پیام به صف
            var messageJson = JsonConvert.SerializeObject(deleteMessage);
            _rabbitMqProducer.SendMessageToQueue(messageJson, "DeleteBookingQueue");

            return Task.FromResult(true); // پیام ارسال شد
        }
    }
}