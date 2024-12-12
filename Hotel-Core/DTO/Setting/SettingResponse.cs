using Entities;

namespace Hotel_Core.DTO;

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
    public static SettingResponse ToSettingResponse(this Setting setting)
    {
        return new SettingResponse()
        {
            Id = setting.Id, 
            MinBookingLength = setting.MinBookingLength, 
            MaxBookingLength = setting.MaxBookingLength, 
            MaxGuestsPerBooking = setting.MaxGuestsPerBooking, 
            BreakfastPrice = setting.BreakfastPrice, 
        };
    }
}