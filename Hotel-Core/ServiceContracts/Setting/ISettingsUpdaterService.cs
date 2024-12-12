using ContactsManager.Core.DTO;
using Hotel_Core.DTO;

namespace ServiceContracts
{
 public interface ISettingUpdaterService
 {
  Task<SettingResponse> UpdateSetting(SettingUpsertRequest settingUpsertRequest);
 }
}