using ContactsManager.Core.DTO;
using Entities;
using Hotel_Core.DTO;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Services
{
 public class CabinsAdderService : ICabinsAdderService
 {
  private readonly ICabinsRepository _cabinsRepository;
  private readonly ILogger<CabinsGetterService> _logger;

  public CabinsAdderService(ICabinsRepository cabinsRepository, ILogger<CabinsGetterService> logger)
  {
   _cabinsRepository = cabinsRepository;
   _logger = logger;
  }

  public async Task<CabinResponse> AddCabin(CabinUpsertRequest cabinAddRequest)
  {
   ArgumentNullException.ThrowIfNull(cabinAddRequest);

   var cabin = cabinAddRequest.ToCabin();

   cabin.Id = Guid.NewGuid();

   await _cabinsRepository.AddCabin(cabin);

   return cabin.ToCabinResponse();
  }
 }
}