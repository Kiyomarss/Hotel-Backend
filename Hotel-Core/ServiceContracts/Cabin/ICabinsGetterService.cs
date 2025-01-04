using ContactsManager.Core.DTO;
using Hotel_Core.DTO;

namespace ServiceContracts;

public interface ICabinsGetterService
{
    Task<CabinResponse?> GetCabinByCabinId(Guid cabinId);
    
    Task<List<CabinResponse>> GetCabins();
}