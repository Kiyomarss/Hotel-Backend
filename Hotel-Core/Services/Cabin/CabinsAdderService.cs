using Hotel_Core.DTO;
using Hotel_Core.ServiceContracts;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;

namespace Services
{
 public class CabinsAdderService : ICabinsAdderService
 {
  private readonly ICabinsRepository _cabinsRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<CabinsGetterService> _logger;

  public CabinsAdderService(
   ICabinsRepository cabinsRepository,
   IUnitOfWork unitOfWork,
   ILogger<CabinsGetterService> logger)
  {
   _cabinsRepository = cabinsRepository;
   _unitOfWork = unitOfWork;
   _logger = logger;
  }

  public async Task<CabinResponse> AddCabin(CabinUpsertRequest cabinAddRequest)
  {
   ArgumentNullException.ThrowIfNull(cabinAddRequest);

   var cabin = cabinAddRequest.ToCabin();

   cabin.Id = Guid.NewGuid();

   await _unitOfWork.BeginTransactionAsync();
   try
   {
    cabin = await _cabinsRepository.AddCabin(cabin);
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
   }
   catch (Exception ex)
   {
    await _unitOfWork.RollbackTransactionAsync();
    _logger.LogError($"Error Adding cabin: {ex.Message}");

    throw;
   }
   
   return cabin.ToCabinResponse();
  }
 }
}