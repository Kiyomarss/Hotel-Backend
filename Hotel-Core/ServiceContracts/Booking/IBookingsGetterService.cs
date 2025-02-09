using ContactsManager.Core.DTO;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;

namespace ServiceContracts;

public interface IBookingsGetterService
{
    Task<GetBookingByBookingIdResult?> GetBookingByBookingId(Guid bookingId);

    Task<Booking?> FindBookingById(Guid bookingId);

    Task<PaginatedResult<BookingResponse>> GetBookings(string? status, string? sortBy, string? sortDirection, int page, int pageSize);
    
    Task<List<GetStaysAfterDateResult>> GetStaysAfterDate(DateTime date);

    Task<List<GetStaysTodayActivityBookingResult>> GetStaysTodayActivity();
}