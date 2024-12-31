using System.ComponentModel.DataAnnotations;
using Hotel_Core.Domain.Entities;

namespace Entities;

public class Guest
{
    [Key]
    public Guid Id { get; set; }
    
    public string FullName { get; set; }
    
    public string Email { get; set; }
    
    public string NationalID { get; set; }
    
    public string Nationality { get; set; }

    public string CountryFlag { get; set; }
    
    public DateTime CreateAt { get; set; }
}