using Entities;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Microsoft.EntityFrameworkCore.Storage;

namespace RepositoryContracts;

public interface IBookingsRepository
{
    Task<Booking> AddBooking(Booking booking);
    
    Task<Booking?> GetBookingByBookingId(Guid bookingId);

    Task<Booking?> FindBookingById(Guid bookingId);

    Task<PaginatedResult<Booking>> GetBookings(string? status, string? sortBy, string? sortDirection, int page, int pageSize);

    Task<List<Booking>> GetBookingsAfterDate(DateTime date);
    
   Task<bool> DeleteBooking(Guid bookingId);

    Task<Booking> UpdateBooking(Booking booking);
    
    Task<List<Booking>> GetStaysAfterDate(DateTime date);

    Task<List<Booking>> GetStaysTodayActivity();
}