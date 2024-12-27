using Entities;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Services
{
 public class CabinsDeleterService : ICabinsDeleterService
 {
  //private field
  private readonly ICabinsRepository _cabinsRepository;
  private readonly ILogger<CabinsGetterService> _logger;

  //constructor
  public CabinsDeleterService(ICabinsRepository cabinsRepository, ILogger<CabinsGetterService> logger)
  {
   _cabinsRepository = cabinsRepository;
   _logger = logger;
  }
  
  public async Task<bool> DeleteCabin(Guid cabinId)
  {
   return await _cabinsRepository.DeleteCabinByCabinId(cabinId);
  }
 }
}
