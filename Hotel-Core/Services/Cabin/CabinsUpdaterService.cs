using ContactsManager.Core.DTO;
using Entities;
using Hotel_Core.DTO;
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
   ArgumentNullException.ThrowIfNull(cabinUpdateRequest);

   var updatedCabin = await _cabinsRepository.UpdateCabin(cabinUpdateRequest);

   return updatedCabin.ToCabinResponse();
  }
 }
}