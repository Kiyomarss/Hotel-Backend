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

  public async Task<Setting> UpdateSetting(Setting setting)
  {
   if (setting == null)
    throw new ArgumentNullException(nameof(setting));
   
   return await _unitOfWork.ExecuteTransactionAsync(async () => await _settingRepository.UpdateSetting(setting));
  }
 }
}