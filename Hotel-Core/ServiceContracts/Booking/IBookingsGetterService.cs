using ContactsManager.Core.DTO;

namespace ServiceContracts;

public interface IBookingsGetterService
{
    Task<BookingResponse?> GetBookingByBookingId(Guid? BookingId);
    Task<List<BookingResponse>> GetAllBookings();
}