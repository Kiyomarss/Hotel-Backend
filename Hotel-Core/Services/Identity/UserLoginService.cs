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

        public async Task<ApplicationUser?> ExternalLoginAsync(UserLoginInfo loginInfo, string? emailFromProvider)
        {
            var user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
            if (user != null)
                return user;

            var email = emailFromProvider ?? $"{loginInfo.ProviderKey}@example.com";

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, loginInfo);
                if (addLoginResult.Succeeded)
                    return existingUser;
                return null;
            }

            var newUser = new ApplicationUser
            {
                UserName = email.Split('@')[0],
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
                return null;

            var addLoginNewUser = await _userManager.AddLoginAsync(newUser, loginInfo);
            return !addLoginNewUser.Succeeded ? null : newUser;
        }
    }
}