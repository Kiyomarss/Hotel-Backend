using Entities;

namespace Hotel_Core.DTO;

public class SettingUpsertRequest
{
    public Guid Id { get; set; }
    
    public int MinBookingLength { get; set; }
    
    public int MaxBookingLength { get; set; }
    
    public int MaxGuestsPerBooking { get; set; }
    
    public int BreakfastPrice { get; set; }
    
    public DateTime CreateAt { get; set; }
    

    public Setting ToSetting()
    {
        return new Setting
        {
            Id = Id, 
            MinBookingLength = MinBookingLength,
            MaxBookingLength = MaxBookingLength,
            MaxGuestsPerBooking = MaxGuestsPerBooking,
            BreakfastPrice = BreakfastPrice,
            CreateAt = CreateAt
        };
    }    
}

