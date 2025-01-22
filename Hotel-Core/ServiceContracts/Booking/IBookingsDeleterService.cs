namespace ServiceContracts
{
 public interface IBookingsDeleterService
 {
  Task InitiateDeleteBooking(Guid bookingId);
 }
}
