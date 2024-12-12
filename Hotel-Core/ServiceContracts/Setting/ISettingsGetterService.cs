using ContactsManager.Core.DTO;
using Hotel_Core.DTO;

namespace ServiceContracts;

public interface ISettingGetterService
{
    Task<SettingResponse> GetSetting();
}