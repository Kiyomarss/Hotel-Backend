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

   Setting savedSetting;
   
   await _unitOfWork.BeginTransactionAsync();
   try
   {
    savedSetting = await _settingRepository.UpdateSetting(settingUpdateRequest.ToSetting());
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
   }
   catch (Exception ex)
   {
    await _unitOfWork.RollbackTransactionAsync();
    _logger.LogError($"Error Updating setting: {ex.Message}");
    
    throw;
   }
   
   return savedSetting.ToSettingResponse();
  }
 }
}