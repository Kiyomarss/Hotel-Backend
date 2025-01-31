using Entities;
using Hotel_Core.DTO;
using Hotel_Core.ServiceContracts;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;

namespace Services
{
 public class SettingUpdaterService : ISettingUpdaterService
 {
  private readonly ISettingRepository _settingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<SettingGetterService> _logger;

  public SettingUpdaterService(
   ISettingRepository settingRepository,
   IUnitOfWork unitOfWork,
   ILogger<SettingGetterService> logger)
  {
   _settingRepository = settingRepository;
   _unitOfWork = unitOfWork;
   _logger = logger;
  }

  public async Task<SettingResponse> UpdateSetting(SettingUpsertRequest settingUpdateRequest)
  {
   if (settingUpdateRequest == null)
    throw new ArgumentNullException(nameof(settingUpdateRequest));
   
   return await _unitOfWork.ExecuteTransactionAsync(async () =>
   {
    var savedSetting = await _settingRepository.UpdateSetting(settingUpdateRequest.ToSetting());

    return savedSetting.ToSettingResponse();
   });
  }
 }
}