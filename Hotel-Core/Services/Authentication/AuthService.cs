using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Hotel_Core.ServiceContracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Hotel_Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityService _identityService;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IIdentityService identityService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _identityService = identityService;
            _configuration = configuration;
        }

        public async Task SignupAsync(SignupRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email, Email = request.Email, PersonName = request.PersonName
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                throw new InvalidOperationException("Signup Failed.");
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new InvalidOperationException("Invalid credentials.");

            var token = await GenerateJwtToken(user);
            return new LoginResult(token, new UserDetails(user.PersonName, user.Email, user.AvatarPath));
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expirationHours = _configuration["Jwt:ExpirationHours"];

            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(expirationHours))
            {
                throw new InvalidOperationException("JWT configuration is incomplete.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(JwtRegisteredClaimNames.Email, user.Email)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(expirationHours)),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _identityService.GetCurrentUserAsync();

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            
            if (!result.Succeeded)
                throw new InvalidOperationException("ChangePassword Failed.");
        }

        public async Task ChangeUserNameAsync(string newUserName)
        {
            var user = await _identityService.GetCurrentUserAsync();

            user.UserName = newUserName;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                throw new InvalidOperationException("Failed to update UserName.");
        }

        public async Task ChangePersonNameAsync(string newPersonName)
        {
            var user = await _identityService.GetCurrentUserAsync();

            user.PersonName = newPersonName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException("Failed to update PersonName.");
        }
        
        public async Task<string> UpdateAvatarAsync(Stream avatarStream)
        {
            var user = await _identityService.GetCurrentUserAsync();

            var avatarPath = await SaveNewAvatarFromStreamAsync(avatarStream);
            user.AvatarPath = avatarPath;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                throw new InvalidOperationException("Update Failed.");

            return avatarPath;
        }

        public virtual async Task<string> SaveNewAvatarFromStreamAsync(Stream fileStream)
        {
            var avatarFolderPath = Path.Combine("wwwroot", "avatars");
            Directory.CreateDirectory(avatarFolderPath);

            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(avatarFolderPath, fileName);

            using (var image = await Image.LoadAsync(fileStream))
            {
                // تبدیل تصویر به JPEG
                await image.SaveAsJpegAsync(filePath, new JpegEncoder{ Quality = 90 });
            }

            return $"/avatars/{fileName}";
        }
        public async Task DeleteUserAsync(string userId)
        {
            var user = await _identityService.GetUserByIdAsync(userId);

            var result = await _userManager.DeleteAsync(user);
            
            if (!result.Succeeded)
                throw new InvalidOperationException("Delete Failed.");
        }
    }
}