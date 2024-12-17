using Microsoft.EntityFrameworkCore.Storage;
using RepositoryContracts;

namespace Hotel_Infrastructure.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly IApplicationDbContext _db;

        public RepositoryBase(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }
    }
}