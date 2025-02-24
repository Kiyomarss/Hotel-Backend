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

        public async Task<bool> AddUserTokenAsync(string userId, string loginProvider, string name, string value)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.SetAuthenticationTokenAsync(user, loginProvider, name, value);
            return result == IdentityResult.Success;
        }

        public async Task<bool> RemoveUserTokenAsync(string userId, string loginProvider, string name)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.RemoveAuthenticationTokenAsync(user, loginProvider, name);
            return result == IdentityResult.Success;
        }

        public async Task<string?> GetUserTokenAsync(string userId, string loginProvider, string name)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return await _userManager.GetAuthenticationTokenAsync(user, loginProvider, name);
        }

        public async Task<bool> TokenExistsAsync(string userId, string loginProvider, string name)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var token = await _userManager.GetAuthenticationTokenAsync(user, loginProvider, name);
            return !string.IsNullOrEmpty(token);
        }
    }
}