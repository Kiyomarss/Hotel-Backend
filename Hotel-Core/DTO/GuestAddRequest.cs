using Entities;

namespace ContactsManager.Core.DTO;

public class GuestAddRequest
{
    public string FullName { get; set; }
    
    public string Email { get; set; }
    
    public string NationalID { get; set; }
    
    public string Nationality { get; set; }

    public string CountryFlag { get; set; }
    
    public DateTime CreateAt { get; set; }
    
    public Guest ToGuest()
    {
        return new Guest
        {
            FullName = FullName,
            Email = Email,
            NationalID = NationalID,
            Nationality = Nationality,
            CountryFlag = CountryFlag,
            CreateAt = CreateAt
        };
    }
}