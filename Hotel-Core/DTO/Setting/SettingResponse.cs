using Entities;

namespace ContactsManager.Core.DTO;

public class SettingResponse
{
    public Guid Id { get; set; }
    
    public int MinBookingLength { get; set; }
    
    public int MaxBookingLength { get; set; }
    
    public int MaxGuestsPerBooking { get; set; }
    
    public int BreakfastPrice { get; set; }
    
    public DateTime CreateAt { get; set; }
}


public static class SettingExtensions
{
    public static SettingResponse ToSettingResponse(this Setting Setting)
    {
        return new SettingResponse()
        {
            Id = Setting.Id, 
            MinBookingLength = Setting.MinBookingLength,
            MaxBookingLength = Setting.MaxBookingLength,
            MaxGuestsPerBooking = Setting.MaxGuestsPerBooking,
            BreakfastPrice = Setting.BreakfastPrice,
            CreateAt = Setting.CreateAt
        };
    }
}