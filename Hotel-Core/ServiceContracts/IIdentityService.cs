using System.Security.Claims;
using ContactsManager.Core.Domain.IdentityEntities;

namespace Hotel_Core.ServiceContracts
{
    public interface IIdentityService
    {
        Task<ApplicationUser?> GetCurrentUserAsync();
    }
}
