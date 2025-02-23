using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;

namespace Hotel_Core.ServiceContracts
{
    public interface IUserLoginService
    {
       Task<IdentityResult> RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey);
        
        Task<ApplicationUser?> FindByLoginAsync(string loginProvider, string providerKey);
        
        Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user);

        Task<ApplicationUser?> ExternalLoginAsync(UserLoginInfo loginInfo, string? emailFromProvider);
    }
}