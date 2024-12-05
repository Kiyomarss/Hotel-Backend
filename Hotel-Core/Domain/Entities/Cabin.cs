using System.ComponentModel.DataAnnotations;
using Hotel_Core.Domain.Entities;

namespace Entities;

public class Cabin
{
    [Key]
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public int MaxCapacity { get; set; }
    
    public int RegularPrice { get; set; }
    
    public int Discount { get; set; }
    
    public string Description { get; private set; }
    
    public byte[] Image { get; set; }
    
    public DateTime CreateAt { get; set; }
    
    public ICollection<Booking>? Bookings { get; set; }
}