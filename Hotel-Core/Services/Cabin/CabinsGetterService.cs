using ServiceContracts;
using ContactsManager.Core.DTO;
using Hotel_Core.DTO;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Services
{
 public class CabinsGetterService : ICabinsGetterService
 {
  //private field
  private readonly ICabinsRepository _cabinsRepository;
  private readonly ILogger<CabinsGetterService> _logger;

  public CabinsGetterService(ICabinsRepository cabinsRepository, ILogger<CabinsGetterService> logger)
  {
   _cabinsRepository = cabinsRepository;
   _logger = logger;
  }
  
  public virtual async Task<CabinResponse?> GetCabinByCabinId(Guid cabinId)
  {
   var cabin = await _cabinsRepository.FindCabinById(cabinId);

   return cabin?.ToCabinResponse();
  }
  
  public virtual async Task<List<CabinResponse>> GetCabins()
  {
   var cabins = await _cabinsRepository.GetCabins();

   return cabins.Select(cabin => cabin.ToCabinResponse()).ToList();
  }
 }
}
