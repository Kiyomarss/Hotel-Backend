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
  
  public virtual async Task<PaginatedResult<BookingsItemResult>> GetBookings(string? status, string? sortBy, string? sortDirection, int page, int pageSize)
  {
   var paginatedResult = await _bookingsRepository.GetBookings(status, sortBy, sortDirection, page, pageSize);

   return new PaginatedResult<BookingsItemResult>
   {
    Items = paginatedResult.Items.Select(s => new BookingsItemResult(s.Status, s.TotalPrice, s.Cabin.Name, s.Guest.CountryFlag, s.CreateAt.ToString())).ToList(),
    TotalCount = paginatedResult.TotalCount
   };
  }
  
 public async Task<List<GetStaysAfterDateResult>> GetStaysAfterDate(DateTime date)
  {
   var bookings = await _bookingsRepository.GetStaysAfterDate(date);

   return bookings.Select(s => new GetStaysAfterDateResult(s.Status, s.CreateAt.ToString())).ToList();
  }
  
  public async Task<List<GetStaysTodayActivityBookingResult>> GetStaysTodayActivity()
  {
   var bookings = await _bookingsRepository.GetStaysTodayActivity();

   return bookings.Select(s => new GetStaysTodayActivityBookingResult(s.Status, s.TotalPrice, s.NumGuests, s.Guest.CountryFlag)).ToList();
  }

  public virtual async Task<BookingResult?> GetBookingByBookingId(Guid bookingId)
  {
   var booking = await _bookingsRepository.GetBookingByBookingId(bookingId);
   
   if (booking == null)
    throw new ArgumentException("Given Booking id doesn't exist");

   return new BookingResult(booking.Status, booking.TotalPrice, booking.Cabin.Name, booking.Guest.CountryFlag, booking.Guest.Nationality);
  }
  
  public virtual async Task<Booking?> FindBookingById(Guid bookingId)
  {
   return await _bookingsRepository.FindBookingById(bookingId);
  }
 }
}
