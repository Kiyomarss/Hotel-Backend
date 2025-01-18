using System.Linq.Expressions;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
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

        public Task<Booking> AddBooking(Booking booking)
        {
            _db.Set<Booking>().Add(booking);

            return Task.FromResult(booking);
        }

        public async Task<Booking?> GetBookingByBookingId(Guid bookingId)
        {
            var booking = await _db.Set<Booking>()
                                   .AsNoTracking()
                                   .Include(b => b.Guest)
                                   .Include(b => b.Cabin)
                                   .FirstOrDefaultAsync(b => b.Id == bookingId);

            return booking;
        }
        
        public async Task<Booking?> FindBookingById(Guid bookingId)
        {
            return await _db.Set<Booking>().FindAsync(bookingId);
        }

        public async Task<bool> DeleteBooking(Guid bookingId)
        {
            var rowsDeleted = await _db.Set<Booking>()
                                       .Where(b => b.Id == bookingId)
                                       .ExecuteDeleteAsync();

            return rowsDeleted > 0;
        }
        
        public async Task<PaginatedResult<Booking>> GetBookings(string? status, string? sortBy, string? sortDirection, int page, int pageSize)
        {
            var query = _db.Set<Booking>().AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }
            
            query = ApplySorting(query, sortBy, sortDirection);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Guest)
                .Include(b => b.Cabin)
                .ToListAsync();

            return new PaginatedResult<Booking>
            {
                Items = bookings,
                TotalCount = totalCount
            };
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
                .Where(b => (b.Status == "checked-in" || b.Status == "unconfirmed"))
                .ToListAsync();
        }
        
        public async Task<List<Booking>> GetStaysTodayActivity()
        {
            return await _db.Set<Booking>()
                .Include(b => b.Guest)
                .Include(b => b.Cabin)
                .Where(b =>
                    (b.Status == "unconfirmed" /*&& b.StartDate.Date == DateTime.UtcNow.Date*/) ||
                    (b.Status == "checked-in" /*&& b.EndDate.Date == DateTime.UtcNow.Date*/))
                .OrderBy(b => b.CreateAt)
                .ToListAsync();
        }

        public Task<Booking> UpdateBooking(Booking booking)
        {
            _db.Set<Booking>().Update(booking);

            return Task.FromResult(booking);
        }
        
        private IQueryable<Booking> ApplySorting(IQueryable<Booking> query, string? sortBy, string? sortDirection)
        {
            if (string.IsNullOrEmpty(sortBy)) return query;

            var sortExpression = sortBy.ToLower() switch
            {
                "startdate" => (Expression<Func<Booking, object>>)(b => b.StartDate),
                "enddate" => b => b.EndDate,
                "totalprice" => b => b.TotalPrice,
                _ => throw new ArgumentException($"Invalid sort column: {sortBy}")
            };

            return sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(sortExpression)
                : query.OrderBy(sortExpression);
        }
    }
}