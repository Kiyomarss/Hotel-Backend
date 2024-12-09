using Entities;
using Hotel_Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repositories
{
    public class CabinsesRepository : ICabinsRepository
    {
        private readonly ApplicationDbContext _db;

        public CabinsesRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Cabin> AddCabin(Cabin cabin)
        {
            _db.Cabins.Add(cabin);
            await _db.SaveChangesAsync();

            return cabin;
        }

        public async Task<List<Cabin>> GetCabins()
        {
            return await _db.Cabins.ToListAsync();
        }

        public async Task<Cabin?> GetCabinByCabinId(Guid cabinId)
        {
            return await _db.Cabins.FirstOrDefaultAsync(temp => temp.Id == cabinId);
        }
        
        public async Task<bool> DeleteCabinByCabinId(Guid cabinId)
        {
            _db.Cabins.RemoveRange(_db.Cabins.Where(temp => temp.Id == cabinId));
            int rowsDeleted = await _db.SaveChangesAsync();

            return rowsDeleted > 0;
        }

        public async Task<Cabin> UpdateCabin(Cabin cabin)
        {
            Cabin? matchingCabin = await _db.Cabins.FirstOrDefaultAsync(temp => temp.Id == cabin.Id);

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