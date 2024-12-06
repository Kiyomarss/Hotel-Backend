using ContactsManager.Core.DTO;

namespace ServiceContracts
{
 public interface ICabinsUpdaterService
 {
  Task<CabinResponse> UpdateCabin(CabinUpsertRequest cabinUpsertRequest);
 }
}