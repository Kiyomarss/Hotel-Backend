using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Hotel_Core.ServiceContracts;
using ContactsManager.Core.Domain.IdentityEntities;

namespace Hotel_Core.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public string? GetLoggedInUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userId = GetLoggedInUserId();
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<bool> CurrentUserHasAnyRoleAsync(params string[] roleNames)
        {
            var userId = GetLoggedInUserId();
            if (string.IsNullOrEmpty(userId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            return userRoles.Any(roleNames.Contains);
        }
        
        public async Task<bool> CurrentUserHasRoleAsync(string roleName)
        {
            var userId = GetLoggedInUserId();
            if (string.IsNullOrEmpty(userId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.IsInRoleAsync(user, roleName);
        }
    }
}