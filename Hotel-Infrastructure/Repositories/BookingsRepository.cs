using Entities;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Hotel_Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Hotel_Infrastructure.Repositories
{
    public class BookingsRepository : RepositoryBase, IBookingsRepository
    {
        private readonly IApplicationDbContext _db;

        public BookingsRepository(IApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Booking> AddBooking(Booking booking)
        {
            _db.Set<Booking>().Add(booking);
            await _db.SaveChangesAsync();

            return booking;
        }

        public async Task<Booking> GetBookingByBookingId(Guid bookingId)
        {
            return await _db.Set<Booking>().Include(b => b.Guest).Include(b => b.Cabin).FirstAsync(temp => temp.Id == bookingId);
        }
        
        public async Task<bool> DeleteBookingByBookingId(Guid bookingId)
        {
            _db.Set<Booking>().RemoveRange(_db.Set<Booking>().Where(temp => temp.Id == bookingId));
            int rowsDeleted = await _db.SaveChangesAsync();

            return rowsDeleted > 0;
        }
        
        public async Task<(List<BookingResponse> Bookings, int TotalCount)> GetBookings(string? status, string? sortBy, string? sortDirection, int page, int pageSize)
        {
            var query = _db.Set<Booking>().AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "startDate":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(b => b.StartDate)
                            : query.OrderBy(b => b.StartDate);
                        break;
                    case "endDate":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(b => b.EndDate)
                            : query.OrderBy(b => b.EndDate);
                        break;
                    case "totalPrice":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(b => b.TotalPrice)
                            : query.OrderBy(b => b.TotalPrice);
                        break;
                    // Add more cases for other sortable properties
                    default:
                        throw new ArgumentException($"Invalid sort column: {sortBy}");
                }

            }

            int totalCount = await query.CountAsync();

            var bookings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Guest)
                .Include(b => b.Cabin)
                .Select(x => x.ToBookingResponse())
                .ToListAsync();

            return (Bookings: bookings, TotalCount: totalCount);
        }

        public async Task<List<Booking>> GetBookingsAfterDate(DateTime date)
        {
            return await _db.Set<Booking>()
                .Include(b => b.Guest)
                .Include(b => b.Cabin)
                .Where(b => b.CreateAt >= date && b.CreateAt <= DateTime.UtcNow)
                .ToListAsync();
        }
        
        public async Task<List<Booking>> GetStaysAfterDate(DateTime date)
        {
            return await _db.Set<Booking>()
                .Include(b => b.Guest)
                .Include(b => b.Cabin)
                .Where(b => (b.Status == "checked-in" || b.Status == "checked-in") && b.StartDate >= date && b.StartDate <= DateTime.UtcNow)
                .ToListAsync();
        }
        
        public async Task<List<Booking>> GetStaysTodayActivity()
        {
            return await _db.Set<Booking>()
                .Include(b => b.Guest)
                .Include(b => b.Cabin)
                .Where(b =>
                    (b.Status == "unconfirmed" && b.StartDate.Date == DateTime.UtcNow.Date) ||
                    (b.Status == "checked-in" && b.EndDate.Date == DateTime.UtcNow.Date))
                .OrderBy(b => b.CreateAt)
                .ToListAsync();
        }

        
        public async Task<Booking> UpdateBooking(Booking booking)
        {
            int countUpdated = await _db.SaveChangesAsync();

            return booking;
        }
    }
}