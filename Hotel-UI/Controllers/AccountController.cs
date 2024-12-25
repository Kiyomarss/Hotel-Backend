using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Hotel_UI.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input." });

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PersonName = request.FullName,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input." });

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            var result = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!result)
                return Unauthorized(new { message = "Invalid credentials." });

            try
            {
                var token = await GenerateJwtToken(user);
                return Ok(new { token, user = new { id = user.Id, fullName = user.PersonName, email = user.Email, avatar = user.AvatarPath } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error. Please try again later." });
            }
            ;
        }
        
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, user.Email),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new (JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            };
            
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var secretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expirationHours = int.TryParse(_configuration["Jwt:ExpirationHours"], out var hours) ? hours : 2;
            var expiration = DateTime.UtcNow.AddHours(expirationHours);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Ok(new { message = "Logout successful." });
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        [Route("[action]")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var isPasswordUpdate = !string.IsNullOrEmpty(request.Password) && !string.IsNullOrEmpty(request.CurrentPassword);
            var isFullNameUpdate = !string.IsNullOrEmpty(request.FullName);

            if (isPasswordUpdate && isFullNameUpdate)
                return BadRequest(new { message = "Only one field can be updated at a time (either password or full name)." });

            if (!isPasswordUpdate && !isFullNameUpdate)
                return BadRequest(new { message = "At least one field (password or full name) must be provided." });

            if (isFullNameUpdate)
            {
                user.PersonName = request.FullName;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errorMessages = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    return BadRequest(new { message = errorMessages });
                }
            }

            else if (isPasswordUpdate)
            {
                if (string.IsNullOrEmpty(request.CurrentPassword))
                    return BadRequest(new { message = "Current password is required for updating password." });

                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.Password);
                if (!passwordChangeResult.Succeeded)
                {
                    var errorMessages = string.Join(", ", passwordChangeResult.Errors.Select(e => e.Description));
                    return BadRequest(new { message = errorMessages });
                }
            }

            return Ok(new
            {
                user = new
                {
                    id = user.Id,
                    fullName = user.PersonName,
                    email = user.Email,
                    avatar = user.AvatarPath
                }
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile avatar)
        {
            var token = Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token is missing." });

            // اعتبارسنجی و استخراج claims از توکن
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken == null)
                return Unauthorized(new { message = "Invalid token." });

            var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value; // از "sub" استفاده کنید

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (avatar == null || avatar.Length == 0)
                return BadRequest(new { message = "No avatar file provided." });

            var avatarPath = await SaveNewAvatarAsync(avatar);

            user.AvatarPath = avatarPath;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(new { message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) });

            return Ok(new { avatar = avatarPath, message = "Avatar updated successfully." });
        }

        private async Task<string> SaveNewAvatarAsync(IFormFile avatar)
        {
            var avatarFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");

            var fileName = Guid.NewGuid() + Path.GetExtension(avatar.FileName);
            
            if (!Directory.Exists(avatarFolderPath))
            {
                Directory.CreateDirectory(avatarFolderPath);
            }

            var filePath = Path.Combine(avatarFolderPath, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            return $"/avatars/{fileName}";
        }
    }

    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        
        public string Password { get; set; }
        
        public string CurrentPassword { get; set; }
    }
}

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class SignupRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }