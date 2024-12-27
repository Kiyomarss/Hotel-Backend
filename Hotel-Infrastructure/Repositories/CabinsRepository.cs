using Entities;
using Hotel_Core.DTO;
using Hotel_Infrastructure.DbContext;
using Hotel_Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repositories
{
    public class CabinsRepository : RepositoryBase, ICabinsRepository
    {
        private readonly IApplicationDbContext _db;

        public CabinsRepository(IApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Cabin> AddCabin(Cabin cabin)
        {
            await _db.Set<Cabin>().AddAsync(cabin);
            await _db.SaveChangesAsync();

            return cabin;
        }

        public async Task<List<Cabin>> GetCabins()
        {
            return await _db.Set<Cabin>().ToListAsync();
        }

        public async Task<Cabin?> GetCabinByCabinId(Guid cabinId)
        {
            return await _db.Set<Cabin>().FindAsync(cabinId);
        }
        
        public async Task<bool> DeleteCabinByCabinId(Guid cabinId)
        {
            var cabin = await _db.Set<Cabin>().FindAsync(cabinId);
            if (cabin == null)
                throw new InvalidOperationException("Cabin with the given ID does not exist.");
            
            _db.Set<Cabin>().Remove(cabin);
            var rowsDeleted = await _db.SaveChangesAsync();

            return rowsDeleted > 0;
        }

        public async Task<Cabin> UpdateCabin(CabinUpsertRequest cabinUpdateRequest)
        {
            var matchingCabin = await _db.Set<Cabin>().FindAsync(cabinUpdateRequest.Id);
            if (matchingCabin == null)
            {
                throw new InvalidOperationException("Cabin with the given ID does not exist.");
            }
            
            matchingCabin.Name = cabinUpdateRequest.Name;
            matchingCabin.MaxCapacity = cabinUpdateRequest.MaxCapacity;
            matchingCabin.RegularPrice = cabinUpdateRequest.RegularPrice;
            matchingCabin.Discount = cabinUpdateRequest.Discount;
            matchingCabin.ImagePath = cabinUpdateRequest.ImagePath;
            matchingCabin.Description = cabinUpdateRequest.Description;

            await _db.SaveChangesAsync();

            return matchingCabin;
        }
    }
}