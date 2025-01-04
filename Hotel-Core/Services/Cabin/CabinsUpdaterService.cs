using Entities;
using Hotel_Core.DTO;
using Hotel_Core.ServiceContracts;
using ServiceContracts;
using RepositoryContracts;
using Microsoft.Extensions.Logging;

namespace Services
{
 public class CabinsUpdaterService : ICabinsUpdaterService
 {
  private readonly ICabinsRepository _cabinsRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<CabinsGetterService> _logger;

  public CabinsUpdaterService(
   ICabinsRepository cabinsRepository,
   IUnitOfWork unitOfWork,
   ILogger<CabinsGetterService> logger)
  {
   _cabinsRepository = cabinsRepository;
   _unitOfWork = unitOfWork;
   _logger = logger;
  }

  public async Task<CabinResponse> UpdateCabin(CabinUpsertRequest cabinUpdateRequest)
  {
   ArgumentNullException.ThrowIfNull(cabinUpdateRequest);

   Cabin updatedCabin;
   
   await _unitOfWork.BeginTransactionAsync();
   try
   {
    updatedCabin = await _cabinsRepository.UpdateCabin(cabinUpdateRequest);
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
   }
   catch (Exception ex)
   {
    await _unitOfWork.RollbackTransactionAsync();
    _logger.LogError($"Error Updating cabin: {ex.Message}");
    
    throw;
   }

   return updatedCabin.ToCabinResponse();
  }
 }
}