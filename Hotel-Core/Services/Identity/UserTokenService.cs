using Microsoft.AspNetCore.Identity;
using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.ServiceContracts;

namespace Hotel_Core.Services
{
    public class UserTokenService : IUserTokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserTokenService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task AddUserTokenAsync(string userId, string loginProvider, string name, string value)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            var token = new IdentityUserToken<string>
            {
                UserId = userId,
                LoginProvider = loginProvider,
                Name = name,
                Value = value
            };

            await _userManager.SetAuthenticationTokenAsync(user, loginProvider, name, value);
        }

        public async Task RemoveUserTokenAsync(string userId, string loginProvider, string name)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            await _userManager.RemoveAuthenticationTokenAsync(user, loginProvider, name);
        }

        public async Task<string?> GetUserTokenAsync(string userId, string loginProvider, string name)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return await _userManager.GetAuthenticationTokenAsync(user, loginProvider, name);
        }

        public async Task<bool> TokenExistsAsync(string userId, string loginProvider, string name)
        {
            return !string.IsNullOrEmpty(await GetUserTokenAsync(userId, loginProvider, name));
        }
    }
}