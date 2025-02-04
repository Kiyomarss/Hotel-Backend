using ContactsManager.Core.DTO;
using Entities;
using Hotel_Core.DTO;

namespace ServiceContracts
{
 public interface ISettingUpdaterService
 {
  Task<Setting> UpdateSetting(Setting setting);
 }
}