using ContactsManager.Core.DTO;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;

namespace ServiceContracts;

public interface IBookingsGetterService
{
    Task<Booking?> GetBookingByBookingId(Guid? bookingId);
    Task<List<BookingResponse>> GetAllBookings();
}