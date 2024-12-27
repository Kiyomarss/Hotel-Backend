using Entities;
using Hotel_Core.DTO;

namespace RepositoryContracts;

public interface ICabinsRepository
{
    Task<Cabin> AddCabin(Cabin cabin);
    
    Task<List<Cabin>> GetCabins();

    Task<Cabin?> GetCabinByCabinId(Guid cabinId);

    Task<bool> DeleteCabinByCabinId(Guid cabinId);

    Task<Cabin> UpdateCabin(CabinUpsertRequest cabinUpsertRequest);
}