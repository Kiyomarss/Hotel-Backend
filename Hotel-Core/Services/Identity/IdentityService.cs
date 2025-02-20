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
        
        public bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(GetLoggedInUserId());
        }
        
        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var userId = GetLoggedInUserId();

            if (string.IsNullOrEmpty(userId))
                throw new KeyNotFoundException("User not found.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return user;
        }

        public async Task<ApplicationUser?> GetCurrentUserWithoutErrorAsync()
        {
            var userId = GetLoggedInUserId();
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return user;
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

        public async Task<IList<string>> GetCurrentUserRolesAsync()
        {
            var userId = GetLoggedInUserId();
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
        }
        
        public async Task<bool> CurrentUserHasAllRolesAsync(params string[] roleNames)
        {
            var userRoles = await GetCurrentUserRolesAsync();
            return roleNames.All(userRoles.Contains);
        }

        public async Task<bool> IsCurrentUserAdminAsync()
        {
            return await CurrentUserHasRoleAsync(Constant.Constant.Role.Admin);
        }
        
        public async Task<bool> HasAccessAsync(string requiredPermission)
        {
            var user = await GetCurrentUserWithoutErrorAsync();
            if (user == null)
                return false;

            var claims = await _userManager.GetClaimsAsync(user);

            return claims.Any(c => c is { Type: Constant.Constant.Claims.FullAccess, Value: "true" }) || claims.Any(c => c.Type == "Permission" && c.Value == requiredPermission);
        }
    }
}