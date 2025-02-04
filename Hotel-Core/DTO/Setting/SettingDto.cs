namespace Hotel_Core.DTO.Setting;

public record SettingResponseDto(SettingDataDto Setting, string Message = "");

public record SettingDataDto(int MaxGuestsPerBooking, int BreakfastPrice, int MinBookingLength, int MaxBookingLength);