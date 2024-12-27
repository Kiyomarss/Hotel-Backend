using Entities;
using Hotel_Core.Domain.Entities;

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
    
    public CabinsDto Cabins { get; set; } = new();
 
    public Guid GuestId { get; set; }

    public GuestsDto Guests { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}

public class GuestsDto
{
    public string? FullName { get; set; }

    public string? Country { get; set; }
    
    public string? CountryFlag { get; set; }
    
    public string? Email { get; set; }
    
    public string? NationalID { get; set; }
}

public class CabinsDto
{
    public string? Name { get; set; }
}


public static class BookingExtensions
{
    public static BookingResponse ToBookingResponse(this Booking? booking)
    {
        return new BookingResponse()
        {
            Id = booking.Id,
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
            Guests = new GuestsDto
            {
                FullName = booking.Guest.FullName,
                Country = booking.Guest.Nationality,
                CountryFlag = booking.Guest.CountryFlag,
                Email = booking.Guest.Email,
                NationalID = booking.Guest.NationalID
            },
            Cabins = new CabinsDto()
            {
                Name = booking.Cabin.Name
            },
            CreatedAt = booking.CreateAt,
        };
    }
}