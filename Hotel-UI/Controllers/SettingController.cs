using Entities;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Setting;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace Hotel_UI.Controllers;

public class SettingsController  : BaseController
{
    private readonly ISettingGetterService _settingGetterService;
    private readonly ISettingUpdaterService _settingUpdaterService;

    public SettingsController(ISettingGetterService settingGetterService, ISettingUpdaterService settingUpdaterService)
    {
        _settingGetterService = settingGetterService;
        _settingUpdaterService = settingUpdaterService;
    }
    
    [HttpPut]
    public async Task<IActionResult> Edit(Setting dto)
    {
        var setting = await _settingUpdaterService.UpdateSetting(dto);

        var response = new SettingResponseDto(new SettingDataDto(setting.MaxGuestsPerBooking, setting.BreakfastPrice, setting.MinBookingLength, setting.MaxBookingLength), "Setting updated successfully");

        return Ok(response);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var setting = await _settingGetterService.GetSetting();
        var response = new SettingResponseDto(new SettingDataDto(setting.MaxGuestsPerBooking, setting.BreakfastPrice, setting.MinBookingLength, setting.MaxBookingLength));

        return Ok(response);
    }
}