using Entities;
using Hotel_Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly ApplicationDbContext _db;

        public SettingRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Setting> GetSetting()
        {
            return await _db.Setting.FirstAsync();
        }
    }
}