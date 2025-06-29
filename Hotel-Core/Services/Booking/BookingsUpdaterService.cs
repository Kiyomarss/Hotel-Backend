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

        public async Task<BookingResult> UpdateBooking(Guid bookingId, JsonPatchDocument<Booking> patchDoc)
        {
            if (patchDoc == null)
                throw new ArgumentNullException(nameof(patchDoc));

            var matchingBooking = await _bookingsRepository.GetBookingByBookingId(bookingId);
            if (matchingBooking == null)
                throw new ArgumentException("Booking ID does not exist");

            patchDoc.ApplyTo(matchingBooking);

            if (!IsValidBooking(matchingBooking))
                throw new InvalidOperationException("Invalid booking data");

            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var updatedBooking = await _bookingsRepository.UpdateBooking(matchingBooking);

                return new BookingResult(updatedBooking.Id, updatedBooking.Status, updatedBooking.TotalPrice, updatedBooking.Cabin.Name, updatedBooking.Guest.CountryFlag, updatedBooking.Guest.Nationality);
            });
        }

        private bool IsValidBooking(Booking booking)
        {
            return booking.StartDate <= booking.EndDate && booking.NumGuests > 0 && booking.NumNights > 0;
        }
    }
}