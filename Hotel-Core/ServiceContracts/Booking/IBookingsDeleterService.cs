namespace ServiceContracts
{
 public interface IBookingsDeleterService
 {
  Task<bool> InitiateDeleteBooking(Guid bookingId);
 }
}
