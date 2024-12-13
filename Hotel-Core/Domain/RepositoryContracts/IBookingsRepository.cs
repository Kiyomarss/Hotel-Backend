using Entities;
using Hotel_Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace RepositoryContracts;

public interface IBookingsRepository
{
    Task<Booking> AddBooking(Booking booking);
    
    Task<List<Booking>> GetAllBookings();

    Task<Booking?> GetBookingByBookingId(Guid bookingId);

    Task<bool> DeleteBookingByBookingId(Guid bookingId);

    Task<Booking> UpdateBooking(Booking booking);

    Task<IDbContextTransaction> BeginTransactionAsync();
}