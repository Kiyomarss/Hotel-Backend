using ServiceContracts;
using ContactsManager.Core.DTO;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
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
  
  public virtual async Task<(List<BookingResponse> Bookings, int TotalCount)> GetBookings(string? status, string? sortBy, string? sortDirection, int page, int pageSize)
  {
   return await _bookingsRepository.GetBookings(status, sortBy, sortDirection, page, pageSize);
  }
  
  public async Task<List<BookingResponse>> GetBookingsAfterDate(DateTime date)
  {
   var bookings = await _bookingsRepository.GetBookingsAfterDate(date);

   return bookings.Select(b => b.ToBookingResponse()).ToList();
  }
  
  public async Task<List<BookingResponse>> GetStaysAfterDate(DateTime date)
  {
   var bookings = await _bookingsRepository.GetStaysAfterDate(date);

   return bookings.Select(s => s.ToBookingResponse()).ToList();
  }
  
  public async Task<List<BookingResponse>> GetStaysTodayActivity()
  {
   var bookings = await _bookingsRepository.GetStaysTodayActivity();

   return bookings.Select(s => s.ToBookingResponse()).ToList();
  }

  public virtual async Task<BookingResponse?> GetBookingByBookingId(Guid bookingId)
  {
   var booking = await _bookingsRepository.GetBookingByBookingId(bookingId);
   
   if (booking == null)
    throw new ArgumentException("Given Booking id doesn't exist");

   return booking.ToBookingResponse();
  }
 }
}
