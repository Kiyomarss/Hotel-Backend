using System.Security.Claims;

namespace Hotel_Core.ServiceContracts
{
    public interface IRoleClaimService
    {
        Task<bool> AddClaimToRoleAsync(string roleName, string claimType, string claimValue);

        Task<bool> RemoveClaimFromRoleAsync(string roleName, string claimType, string claimValue);

        Task<IList<Claim>> GetClaimsByRoleAsync(string roleName);

        Task<bool> RoleHasClaimAsync(string roleName, string claimType, string claimValue);
    }
}