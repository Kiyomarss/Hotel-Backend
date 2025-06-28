using ContactsManager.Core.DTO;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;

namespace ServiceContracts;

public interface IBookingsGetterService
{
    Task<BookingResult?> GetBookingByBookingId(Guid bookingId);

    Task<Booking?> FindBookingById(Guid bookingId);

    Task<PaginatedResult<BookingsItemResult>> GetBookings(string? status, string? sortBy, string? sortDirection, int page, int pageSize);
    
    Task<List<GetBookingsAfterDateResult>> GetBookingsAfterDate(DateTime date);
    
    Task<List<GetStaysAfterDateResult>> GetStaysAfterDate(DateTime date);

    Task<List<GetStaysTodayActivityBookingResult>> GetStaysTodayActivity();
}