using Hotel_Core.ServiceContracts;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Services
{
    public class BookingsDeleterService : IBookingsDeleterService
    {
        //private field
        private readonly IBookingsRepository _bookingsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingsGetterService> _logger;

        //constructor
        public BookingsDeleterService(IBookingsRepository bookingsRepository, IUnitOfWork unitOfWork, ILogger<BookingsGetterService> logger)
        {
            _bookingsRepository = bookingsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> DeleteBooking(Guid bookingId)
        {
            var booking = await _bookingsRepository.FindBookingById(bookingId);
            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {bookingId} does not exist.");

            return await _unitOfWork.ExecuteTransactionAsync(async () => await _bookingsRepository.DeleteBooking(bookingId));
        }
    }
}