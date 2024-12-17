using Entities;
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
            _db.Set<Cabin>().Add(cabin);
            await _db.SaveChangesAsync();

            return cabin;
        }

        public async Task<List<Cabin>> GetCabins()
        {
            return await _db.Set<Cabin>().ToListAsync();
        }

        public async Task<Cabin?> GetCabinByCabinId(Guid cabinId)
        {
            return await _db.Set<Cabin>().FirstOrDefaultAsync(temp => temp.Id == cabinId);
        }
        
        public async Task<bool> DeleteCabinByCabinId(Guid cabinId)
        {
            _db.Set<Cabin>().RemoveRange(_db.Set<Cabin>().Where(temp => temp.Id == cabinId));
            int rowsDeleted = await _db.SaveChangesAsync();

            return rowsDeleted > 0;
        }

        public async Task<Cabin> UpdateCabin(Cabin cabin)
        {
            Cabin? matchingCabin = await _db.Set<Cabin>().FirstOrDefaultAsync(temp => temp.Id == cabin.Id);

            if (matchingCabin == null)
                return cabin;

            matchingCabin.Name = cabin.Name;
            matchingCabin.MaxCapacity = cabin.MaxCapacity;
            matchingCabin.RegularPrice = cabin.RegularPrice;
            matchingCabin.Discount = cabin.Discount;
            matchingCabin.ImagePath = cabin.ImagePath;
            matchingCabin.Description = cabin.Description;

            int countUpdated = await _db.SaveChangesAsync();

            return matchingCabin;
        }
    }
}