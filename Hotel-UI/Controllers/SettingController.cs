using Hotel_Core.DTO;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace Hotel_UI.Controllers;

[Route("[controller]")]
public class SettingsController  : Controller
{
    private readonly ISettingGetterService _settingGetterService;
    private readonly ISettingUpdaterService _settingUpdaterService;

    public SettingsController(ISettingGetterService settingGetterService, ISettingUpdaterService settingUpdaterService)
    {
        _settingGetterService = settingGetterService;
        _settingUpdaterService = settingUpdaterService;
    }
    
    [HttpPut]
    [Route("[action]")]
    public async Task<IActionResult> Edit([FromBody] SettingUpsertRequest dto)
    {
        SettingResponse updatedSetting = await _settingUpdaterService.UpdateSetting(dto);

        return Ok(new
        {
            Message = "Setting updated successfully",
            Setting = updatedSetting
        });
    }
    
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetSettings()
    {
        var setting = await _settingGetterService.GetSetting();
        return Json(new { data = setting });
    }
}