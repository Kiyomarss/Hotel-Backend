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

   var savedSetting = await _settingRepository.UpdateSetting(settingUpdateRequest.ToSetting());

   return savedSetting.ToSettingResponse();
  }
 }
}