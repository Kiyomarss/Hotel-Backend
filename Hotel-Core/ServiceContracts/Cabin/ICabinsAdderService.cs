using ContactsManager.Core.DTO;
using Hotel_Core.DTO;

namespace ServiceContracts
{
 public interface ICabinsAdderService
 {
  Task<CabinResponse> AddCabin(CabinUpsertRequest cabinUpsertRequest);
 }
}