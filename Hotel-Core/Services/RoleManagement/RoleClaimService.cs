using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Hotel_Core.Services
{
    public class RoleClaimService : IRoleClaimService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleClaimService(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<bool> AddClaimToRoleAsync(string roleName, string claimType, string claimValue)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return false;

            var claim = new Claim(claimType, claimValue);
            var result = await _roleManager.AddClaimAsync(role, claim);
            return result.Succeeded;
        }

        public async Task<bool> RemoveClaimFromRoleAsync(string roleName, string claimType, string claimValue)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return false;

            var claims = await _roleManager.GetClaimsAsync(role);
            var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType && c.Value == claimValue);

            if (claimToRemove == null)
                return false;

            var result = await _roleManager.RemoveClaimAsync(role, claimToRemove);
            return result.Succeeded;
        }

        public async Task<IList<Claim>> GetClaimsByRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            return role != null ? await _roleManager.GetClaimsAsync(role) : new List<Claim>();
        }

        public async Task<bool> RoleHasClaimAsync(string roleName, string claimType, string claimValue)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return false;

            var claims = await _roleManager.GetClaimsAsync(role);
            return claims.Any(c => c.Type == claimType && c.Value == claimValue);
        }
    }
}