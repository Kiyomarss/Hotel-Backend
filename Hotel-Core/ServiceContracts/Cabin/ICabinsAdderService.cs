using ContactsManager.Core.DTO;

namespace ServiceContracts
{
 public interface ICabinsAdderService
 {
  Task<CabinResponse> AddCabin(CabinUpsertRequest cabinUpsertRequest);
 }
}