using Microsoft.AspNetCore.JsonPatch;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class BookingsUpdaterService : IBookingsUpdaterService
    {
        private readonly IBookingsRepository _bookingsRepository;
        private readonly ILogger<BookingsUpdaterService> _logger;

        public BookingsUpdaterService(IBookingsRepository bookingsRepository, ILogger<BookingsUpdaterService> logger)
        {
            _bookingsRepository = bookingsRepository;
            _logger = logger;
        }

        public async Task<BookingResponse> UpdateBooking(Guid bookingId, JsonPatchDocument<Booking> patchDoc)
        {
            if (patchDoc == null)
                throw new ArgumentNullException(nameof(patchDoc));

            var booking = await _bookingsRepository.GetBookingByBookingId(bookingId);
            if (booking == null)
                throw new ArgumentException("Booking ID does not exist");

            patchDoc.ApplyTo(booking);

            if (!IsValidBooking(booking))
                throw new InvalidOperationException("Invalid booking data");

            await _bookingsRepository.UpdateBooking(booking);
            return booking.ToBookingResponse();
        }

        private bool IsValidBooking(Booking booking)
        {
            return booking.StartDate <= booking.EndDate && booking.NumGuests > 0 && booking.NumNights > 0;
        }
    }
}