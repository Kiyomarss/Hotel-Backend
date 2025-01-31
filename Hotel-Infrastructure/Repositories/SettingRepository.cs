using Entities;
using Hotel_Infrastructure.DbContext;
using Hotel_Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repositories
{
    public class SettingRepository : RepositoryBase, ISettingRepository
    {
        private readonly IApplicationDbContext _db;

        public SettingRepository(IApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Setting?> GetSetting()
        {
            return await _db.Set<Setting>().SingleOrDefaultAsync();
        }

        public async Task<Setting> UpdateSetting(Setting setting)
        {
            var matchingSetting = await _db.Set<Setting>().SingleAsync();
            
            matchingSetting.MinBookingLength = setting.MinBookingLength;
            matchingSetting.MaxBookingLength = setting.MaxBookingLength;
            matchingSetting.MaxGuestsPerBooking = setting.MaxGuestsPerBooking;
            matchingSetting.BreakfastPrice = setting.BreakfastPrice;
            
            return matchingSetting;
        }
    }
}