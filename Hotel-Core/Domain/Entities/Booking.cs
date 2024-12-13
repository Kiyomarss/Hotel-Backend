using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entities;

namespace Hotel_Core.Domain.Entities;

public class Booking
{
    [Key]
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

    [ForeignKey("CabinId")]
    public virtual Cabin Cabin { get; set; }
    
    public Guid GuestId { get; set; }

    [ForeignKey("GuestId")]
    public virtual  Guest Guest { get; set; }

    public DateTime CreateAt { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}