using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Hotel_Core.Services
{
    public class UserLoginService : IUserLoginService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserLoginService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo loginInfo)
        {
            return await _userManager.AddLoginAsync(user, loginInfo);
        }

        public async Task<IdentityResult> RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey)
        {
            return await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
        }

        public async Task<ApplicationUser?> FindByLoginAsync(string loginProvider, string providerKey)
        {
            return await _userManager.FindByLoginAsync(loginProvider, providerKey);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
        {
            return await _userManager.GetLoginsAsync(user);
        }

        public async Task<ApplicationUser?> ExternalLoginAsync(UserLoginInfo loginInfo)
        {
            var user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);

            if (user != null)
                return user;

            var newUser = new ApplicationUser
            {
                UserName = loginInfo.ProviderKey, Email = loginInfo.ProviderKey + "@example.com",
            };

            var result = await _userManager.CreateAsync(newUser);
            if (result.Succeeded)
            {
                await _userManager.AddLoginAsync(newUser, loginInfo);

                return newUser;
            }

            return null;
        }
    }
}