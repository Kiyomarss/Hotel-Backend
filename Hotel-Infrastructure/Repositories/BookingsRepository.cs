using Entities;
using Hotel_Core.Domain.Entities;
using Hotel_Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Hotel_Infrastructure.Repositories
{
    public class BookingsRepository : IBookingsRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Booking> AddBooking(Booking booking)
        {
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return booking;
        }

        public async Task<List<Booking>> GetAllBookings()
        {
            return await _db.Bookings.ToListAsync();
        }

        public async Task<Booking?> GetBookingByBookingId(Guid bookingId)
        {
            return await _db.Bookings.FirstOrDefaultAsync(temp => temp.Id == bookingId);
        }
        
        public async Task<bool> DeleteBookingByBookingId(Guid bookingId)
        {
            _db.Bookings.RemoveRange(_db.Bookings.Where(temp => temp.Id == bookingId));
            int rowsDeleted = await _db.SaveChangesAsync();

            return rowsDeleted > 0;
        }
    }
}