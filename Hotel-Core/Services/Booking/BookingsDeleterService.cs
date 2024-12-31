using Entities;
using Hotel_Core.Domain.Entities;
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
  private readonly ILogger<BookingsGetterService> _logger;

  //constructor
  public BookingsDeleterService(IBookingsRepository bookingsRepository, ILogger<BookingsGetterService> logger)
  {
   _bookingsRepository = bookingsRepository;
   _logger = logger;
  }

  public async Task<bool> DeleteBooking(Guid bookingId)
  {
   var booking = await _bookingsRepository.FindBookingById(bookingId);
   if (booking == null)
    throw new KeyNotFoundException($"Booking with ID {bookingId} does not exist.");

   return await _bookingsRepository.DeleteBooking(booking);
  }
 }
}
