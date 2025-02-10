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

namespace Hotel_Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<ResultDto<string>> SignupAsync(SignupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return ResultDto<string>.Failure("Email and password are required.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PersonName = request.PersonName
            };

            var result = await _userManager.CreateAsync(user);

            return result.Succeeded
                       ? ResultDto<string>.Success(null)
                       : ResultDto<string>.Failure("Update Failed.");
        }


        public async Task<ResultDto<LoginResult>> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return ResultDto<LoginResult>.Failure("Email and password are required.");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return ResultDto<LoginResult>.Failure("Invalid credentials.");

            var token = await GenerateJwtToken(user);
            var loginResult = new LoginResult(token, new UserDetails(user.PersonName, user.Email, user.AvatarPath));

            return ResultDto<LoginResult>.Success(loginResult);
        }


        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, user.Email),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new (JwtRegisteredClaimNames.NameId, user.Id.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var secretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expiration = DateTime.UtcNow.AddHours(int.TryParse(_configuration["Jwt:ExpirationHours"], out var hours) ? hours : 2);

            var token = new JwtSecurityToken(issuer, audience, claims, expires: expiration, signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
    public async Task<ResultDto<UserDto>> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ResultDto<UserDto>.Failure("User not found.");

        var isPasswordUpdate = !string.IsNullOrEmpty(request.Password) && !string.IsNullOrEmpty(request.CurrentPassword);
        var isFullNameUpdate = !string.IsNullOrEmpty(request.FullName);

        if (isPasswordUpdate == isFullNameUpdate)
            return ResultDto<UserDto>.Failure("Only one field can be updated at a time (either password or full name).");

        if (isFullNameUpdate)
        {
            user.PersonName = request.FullName!;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ResultDto<UserDto>.Failure(string.Join(", ", updateResult.Errors.Select(e => e.Description)));
        }
        else if (isPasswordUpdate)
        {
            var passwordChangeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword!, request.Password!);
            if (!passwordChangeResult.Succeeded)
                return ResultDto<UserDto>.Failure(string.Join(", ", passwordChangeResult.Errors.Select(e => e.Description)));
        }

        return ResultDto<UserDto>.Success(new UserDto(user.PersonName, user.Email, user.AvatarPath));
    }

    public async Task<ResultDto<string>> UpdateAvatarAsync(string userId, Stream avatarStream)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ResultDto<string>.Failure("User not found.");

        var avatarPath = await SaveNewAvatarFromStreamAsync(avatarStream);
        user.AvatarPath = avatarPath;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return ResultDto<string>.Failure("Update Failed.");

        return ResultDto<string>.Success(avatarPath);
    }

    private async Task<string> SaveNewAvatarFromStreamAsync(Stream fileStream)
    {
        var avatarFolderPath = Path.Combine("wwwroot", "avatars");
        Directory.CreateDirectory(avatarFolderPath);

        var fileName = $"{Guid.NewGuid()}.jpg";
        var filePath = Path.Combine(avatarFolderPath, fileName);

        await using var outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(outputStream);

        return $"/avatars/{fileName}";
    }

    }
}
