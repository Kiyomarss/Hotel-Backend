using ContactsManager.Core.DTO;

namespace ServiceContracts;

public interface ICabinsGetterService
{
    Task<CabinResponse?> GetCabinByCabinId(Guid? cabinId);
    Task<List<CabinResponse>> GetAllCabins();
}