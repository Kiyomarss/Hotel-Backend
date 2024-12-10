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
  public BookingsDeleterService(IBookingsRepository BookingsRepository, ILogger<BookingsGetterService> logger)
  {
   _bookingsRepository = BookingsRepository;
   _logger = logger;
  }


  public async Task<bool> DeleteBooking(Guid? bookingId)
  {
   if (bookingId == null)
   {
    throw new ArgumentNullException(nameof(bookingId));
   }

   Booking? booking = await _bookingsRepository.GetBookingByBookingId(bookingId.Value);
   if (booking == null)
    return false;

   await _bookingsRepository.DeleteBookingByBookingId(bookingId.Value);

   return true;
  }
 }
}
