using ContactsManager.Core.DTO;

namespace ServiceContracts;

public interface ISettingGetterService
{
    Task<SettingResponse> GetSetting();
}