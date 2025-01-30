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

   return await _unitOfWork.ExecuteTransactionAsync(async () =>
   {
    var updatedCabin = await _cabinsRepository.UpdateCabin(cabinUpdateRequest);
    return updatedCabin.ToCabinResponse();
   });
  }
 }
}