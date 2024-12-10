using Entities;

namespace ContactsManager.Core.DTO;

public class SettingAddRequest
{
    public int MinBookingLength { get; set; }
    
    public int MaxBookingLength { get; set; }
    
    public int MaxGuestsPerBooking { get; set; }
    
    public int BreakfastPrice { get; set; }
    
    public DateTime CreateAt { get; set; }
    
    public Setting ToSetting()
    {
        return new Setting
        {
            MinBookingLength = MinBookingLength,
            MaxBookingLength = MaxBookingLength,
            MaxGuestsPerBooking = MaxGuestsPerBooking,
            BreakfastPrice = BreakfastPrice,
            CreateAt = CreateAt
        };
    }
}