using Hotel_Core.DTO;
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
    public async Task<IActionResult> Edit(SettingUpsertRequest dto)
    {
        SettingResponse updatedSetting = await _settingUpdaterService.UpdateSetting(dto);

        return Ok(new
        {
            Message = "Setting updated successfully",
            Setting = updatedSetting
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var setting = await _settingGetterService.GetSetting();
        return Ok(new { data = setting });
    }
}