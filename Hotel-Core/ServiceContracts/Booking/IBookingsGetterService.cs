using ContactsManager.Core.DTO;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;

namespace ServiceContracts;

public interface IBookingsGetterService
{
    Task<BookingResponse?> GetBookingByBookingId(Guid bookingId);

    Task<(List<BookingResponse> Bookings, int TotalCount)> GetBookings(string? status, string? sortBy, string? sortDirection, int page, int pageSize);
    
    Task<List<BookingResponse>> GetBookingsAfterDate(DateTime date);

    Task<List<BookingResponse>> GetStaysAfterDate(DateTime date);

    Task<List<BookingResponse>> GetStaysTodayActivity();
}