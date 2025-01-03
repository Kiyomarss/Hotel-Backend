using Microsoft.AspNetCore.JsonPatch;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Hotel_Core.ServiceContracts;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class BookingsUpdaterService : IBookingsUpdaterService
    {
        private readonly IBookingsRepository _bookingsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingsUpdaterService> _logger;

        public BookingsUpdaterService(
            IBookingsRepository bookingsRepository,
            IUnitOfWork unitOfWork,
            ILogger<BookingsUpdaterService> logger)
        {
            _bookingsRepository = bookingsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BookingResponse> UpdateBooking(Guid bookingId, JsonPatchDocument<Booking> patchDoc)
        {
            if (patchDoc == null)
                throw new ArgumentNullException(nameof(patchDoc));

            var matchingBooking = await _bookingsRepository.GetBookingByBookingId(bookingId);
            if (matchingBooking == null)
                throw new ArgumentException("Booking ID does not exist");

            patchDoc.ApplyTo(matchingBooking);

            if (!IsValidBooking(matchingBooking))
                throw new InvalidOperationException("Invalid booking data");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return matchingBooking.ToBookingResponse();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError($"Error updating booking: {ex.Message}");
                throw;
            }
        }

        private bool IsValidBooking(Booking booking)
        {
            return booking.StartDate <= booking.EndDate && booking.NumGuests > 0 && booking.NumNights > 0;
        }
    }
}