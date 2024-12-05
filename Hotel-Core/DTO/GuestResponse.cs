using Entities;

namespace ContactsManager.Core.DTO;

public class GuestResponse
{
    public Guid Id { get; set; }
    
    public string FullName { get; set; }
    
    public string Email { get; set; }
    
    public string NationalID { get; set; }
    
    public string Nationality { get; set; }

    public string CountryFlag { get; set; }
    
    public DateTime CreateAt { get; set; }
}


public static class GuestExtensions
{
    public static GuestResponse ToGuestResponse(this Guest Guest)
    {
        return new GuestResponse()
        {
            Id = Guest.Id, 
            FullName = Guest.FullName, 
            Email = Guest.Email, 
            NationalID = Guest.NationalID, 
            Nationality = Guest.Nationality, 
            CountryFlag = Guest.CountryFlag, 
            CreateAt = Guest.CreateAt
        };
    }
}