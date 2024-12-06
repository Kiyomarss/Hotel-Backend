using Entities;

namespace RepositoryContracts;

public interface ICabinsRepository
{
    Task<Cabin> AddCabin(Cabin cabin);
    
    Task<List<Cabin>> GetCabins();

    Task<Cabin?> GetCabinByCabinId(Guid cabinId);

    Task<bool> DeleteCabinByCabinId(Guid cabinId);
}