namespace Hotel_Core.DTO;

public class BookingResponse
{
    public Guid Id { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public int NumNights { get; set; }
    
    public int NumGuests { get; set; }
    
    public int CabinPrice { get; set; }
    
    public int ExtrasPrice { get; set; }
    
    public int TotalPrice { get; set; }
    
    public string Status { get; set; }
    
    public bool HasBreakfast { get; set; }
    
    public bool IsPaid { get; set; }
    
    public string Observations { get; set; }
    
    public Guid CabinId { get; set; }
    
    public Guid GuestId { get; set; }

    public DateTime CreateAt { get; set; }
}


public static class BookingExtensions
{
    public static BookingResponse ToBookingResponse(this Domain.Entities.Booking booking)
    {
        return new BookingResponse()
        {
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            NumNights = booking.NumNights,
            NumGuests = booking.NumGuests,
            CabinPrice = booking.CabinPrice,
            ExtrasPrice = booking.ExtrasPrice,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            HasBreakfast = booking.HasBreakfast,
            IsPaid = booking.IsPaid,
            Observations = booking.Observations,
            CabinId = booking.CabinId,
            GuestId = booking.GuestId,
            CreateAt = booking.CreateAt,
        };
    }
}