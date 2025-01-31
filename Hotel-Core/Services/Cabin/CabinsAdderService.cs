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

   return await _unitOfWork.ExecuteTransactionAsync(async () =>
   {
    cabin = await _cabinsRepository.AddCabin(cabin);

    return cabin.ToCabinResponse();
   });
  }
 }
}