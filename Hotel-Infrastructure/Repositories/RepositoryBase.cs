using Hotel_Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hotel_Infrastructure.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly ApplicationDbContext _db;

        public RepositoryBase(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }
    }
}