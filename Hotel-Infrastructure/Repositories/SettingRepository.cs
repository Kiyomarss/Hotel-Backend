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
            return await _db.Setting.SingleAsync();
        }

        public async Task<Setting> UpdateSetting(Setting setting)
        {
            Setting matchingSetting = await _db.Setting.SingleAsync();
            
            matchingSetting.MinBookingLength = setting.MinBookingLength;
            matchingSetting.MaxBookingLength = setting.MaxBookingLength;
            matchingSetting.MaxGuestsPerBooking = setting.MaxGuestsPerBooking;
            matchingSetting.BreakfastPrice = setting.BreakfastPrice;

            int countUpdated = await _db.SaveChangesAsync();

            return matchingSetting;
        }
    }
}