using ContactsManager.Core.DTO;
using Entities;
using Hotel_Core.DTO;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Services
{
 public class SettingUpdaterService : ISettingUpdaterService
 {
  private readonly ISettingRepository _settingRepository;
  private readonly ILogger<SettingGetterService> _logger;

  public SettingUpdaterService(ISettingRepository settingRepository, ILogger<SettingGetterService> logger)
  {
   _settingRepository = settingRepository;
   _logger = logger;
  }

  public async Task<SettingResponse> UpdateSetting(SettingUpsertRequest settingUpdateRequest)
  {
   if (settingUpdateRequest == null)
    throw new ArgumentNullException(nameof(settingUpdateRequest));
   
   var matchingSetting = await _settingRepository.GetSetting();
   
   matchingSetting.MinBookingLength = settingUpdateRequest.MinBookingLength;
   matchingSetting.MaxBookingLength = settingUpdateRequest.MaxBookingLength;
   matchingSetting.MaxGuestsPerBooking = settingUpdateRequest.MaxGuestsPerBooking;
   matchingSetting.BreakfastPrice = settingUpdateRequest.BreakfastPrice;
   
   await _settingRepository.UpdateSetting(matchingSetting);

   return matchingSetting.ToSettingResponse();
  }
 }
}