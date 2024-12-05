using ServiceContracts;
using ContactsManager.Core.DTO;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Services
{
 public class SettingGetterService : ISettingGetterService
 {
  //private field
  private readonly ISettingRepository _settingRepository;
  private readonly ILogger<SettingGetterService> _logger;

  public SettingGetterService(ISettingRepository SettingRepository, ILogger<SettingGetterService> logger)
  {
   _settingRepository = SettingRepository;
   _logger = logger;
  }
  
  public virtual async Task<SettingResponse> GetSetting()
  {
   var setting = await _settingRepository.GetSetting();

   return setting.ToSettingResponse();
  }
 }
}
