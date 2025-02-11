using System.Security.Claims;
using ContactsManager.Core.Domain.IdentityEntities;

namespace Hotel_Core.ServiceContracts
{
    public interface IIdentityService
    {
        Task<ApplicationUser?> GetCurrentUserAsync();

        Task<bool> CurrentUserHasAnyRoleAsync(params string[] roleNames);

        Task<bool> CurrentUserHasRoleAsync(string roleName);

        bool IsUserLoggedIn();

        Task<IList<string>> GetCurrentUserRolesAsync();

        Task<bool> CurrentUserHasAllRolesAsync(params string[] roleNames);
        
        Task<bool> IsCurrentUserAdminAsync();

        Task<bool> HasAccessAsync(string requiredPermission);
    }
}
