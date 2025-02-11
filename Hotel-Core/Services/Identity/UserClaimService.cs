using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Hotel_Core.ServiceContracts;
using ContactsManager.Core.Domain.IdentityEntities;

namespace Hotel_Core.Services
{
    public class UserClaimService : IUserClaimService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserClaimService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> AddClaimToUserAsync(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var claim = new Claim(claimType, claimValue);
            var result = await _userManager.AddClaimAsync(user, claim);
            return result.Succeeded;
        }

        public async Task<bool> RemoveClaimFromUserAsync(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var claims = await _userManager.GetClaimsAsync(user);
            var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType && c.Value == claimValue);

            if (claimToRemove == null)
                return false;

            var result = await _userManager.RemoveClaimAsync(user, claimToRemove);
            return result.Succeeded;
        }

        public async Task<IList<Claim>> GetClaimsByUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? await _userManager.GetClaimsAsync(user) : new List<Claim>();
        }

        public async Task<bool> UserHasClaimAsync(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.Any(c => c.Type == claimType && c.Value == claimValue);
        }
    }
}