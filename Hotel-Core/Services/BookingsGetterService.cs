using ServiceContracts;
using ContactsManager.Core.DTO;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Services
{
 public class BookingsGetterService : IBookingsGetterService
 {
  //private field
  private readonly IBookingsRepository _bookingsRepository;
  private readonly ILogger<BookingsGetterService> _logger;

  public BookingsGetterService(IBookingsRepository bookingsRepository, ILogger<BookingsGetterService> logger)
  {
   _bookingsRepository = bookingsRepository;
   _logger = logger;
  }
  
  public virtual async Task<BookingResponse?> GetBookingByBookingId(Guid? bookingId)
  {
   if (bookingId == null)
    return null;

   var booking = await _bookingsRepository.GetBookingByBookingId(bookingId.Value);

   return booking?.ToBookingResponse();
  }
  
  public virtual async Task<List<BookingResponse>> GetAllBookings()
  {
   var bookings = await _bookingsRepository.GetAllBookings();

   return bookings.Select(booking => booking.ToBookingResponse()).ToList();
  }
 }
}
