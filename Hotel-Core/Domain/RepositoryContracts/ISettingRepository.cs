using Entities;

namespace RepositoryContracts;

public interface ISettingRepository
{
    Task<Setting> GetSetting();
}