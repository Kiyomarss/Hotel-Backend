using Entities;
using ServiceContracts;
using Hotel_Core.DTO;
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

  public SettingGetterService(ISettingRepository settingRepository, ILogger<SettingGetterService> logger)
  {
   _settingRepository = settingRepository;
   _logger = logger;
  }
  
  public virtual async Task<Setting> GetSetting()
  {
   var setting = await _settingRepository.GetSetting();
    
   if (setting == null)
   {
    throw new InvalidOperationException("No settings found");
   }

   return setting;
  }

 }
}
