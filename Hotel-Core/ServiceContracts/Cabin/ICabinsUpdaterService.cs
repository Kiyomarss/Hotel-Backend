using ContactsManager.Core.DTO;
using Hotel_Core.DTO;

namespace ServiceContracts
{
 public interface ICabinsUpdaterService
 {
  Task<CabinResponse> UpdateCabin(CabinUpsertRequest cabinUpsertRequest);
 }
}