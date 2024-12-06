using ContactsManager.Core.DTO;
using Entities;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Services
{
 public class CabinsUpdaterService : ICabinsUpdaterService
 {
  private readonly ICabinsRepository _cabinsRepository;
  private readonly ILogger<CabinsGetterService> _logger;

  public CabinsUpdaterService(ICabinsRepository cabinsRepository, ILogger<CabinsGetterService> logger)
  {
   _cabinsRepository = cabinsRepository;
   _logger = logger;
  }

  public async Task<CabinResponse> UpdateCabin(CabinUpsertRequest cabinUpdateRequest)
  {
   if (cabinUpdateRequest == null)
    throw new ArgumentNullException(nameof(cabinUpdateRequest));
   
   Cabin? matchingCabin = await _cabinsRepository.GetCabinByCabinId(cabinUpdateRequest.Id);
   if (matchingCabin == null)
   {
    throw new ArgumentException("Given Cabin id doesn't exist");
   }

   matchingCabin.Name = cabinUpdateRequest.Name;
   matchingCabin.MaxCapacity = cabinUpdateRequest.MaxCapacity;
   matchingCabin.RegularPrice = cabinUpdateRequest.RegularPrice;
   matchingCabin.Discount = cabinUpdateRequest.Discount;
   matchingCabin.Image = cabinUpdateRequest.Image;
   matchingCabin.Description = cabinUpdateRequest.Description;
   
   await _cabinsRepository.UpdateCabin(matchingCabin);

   return matchingCabin.ToCabinResponse();
  }
 }
}