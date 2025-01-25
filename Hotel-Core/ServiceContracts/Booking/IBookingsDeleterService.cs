namespace ServiceContracts
{
    public interface IBookingsDeleterService
    {
        Task<bool> DeleteBooking(Guid bookingId);
    }
}