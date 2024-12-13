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

            Booking? matchingBooking = await _bookingsRepository.GetBookingByBookingId(bookingId);
            if (matchingBooking == null)
                throw new ArgumentException("Given Booking id doesn't exist");

            patchDoc.ApplyTo(matchingBooking);

            if (!IsValidBooking(matchingBooking))
                throw new InvalidOperationException("Booking is invalid after patch application");

            await using var transaction = await _bookingsRepository.BeginTransactionAsync();
            try
            {
                await _bookingsRepository.UpdateBooking(matchingBooking);

                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new DbUpdateConcurrencyException("Booking has been modified by another user.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return matchingBooking.ToBookingResponse();
        }

        private bool IsValidBooking(Booking booking)
        {
            return booking.StartDate <= booking.EndDate && booking.NumGuests > 0 && booking.NumNights > 0;
        }
    }
}