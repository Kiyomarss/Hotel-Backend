using Hotel_Core.ServiceContracts;
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
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<CabinsGetterService> _logger;

  //constructor
  public CabinsDeleterService(ICabinsRepository cabinsRepository, IUnitOfWork unitOfWork, ILogger<CabinsGetterService> logger)
  {
   _cabinsRepository = cabinsRepository;
   _unitOfWork = unitOfWork;
   _logger = logger;
  }
  
  public async Task<bool> DeleteCabin(Guid cabinId)
  {
   var cabin = await _cabinsRepository.FindCabinById(cabinId);
   if (cabin == null)
    throw new KeyNotFoundException($"cabin with ID {cabinId} does not exist.");

   return await _unitOfWork.ExecuteTransactionAsync(async () => await _cabinsRepository.DeleteCabin(cabinId));
  }
 }
}
