using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Hotel_UI.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                return Unauthorized(new { message = "Invalid email or password." });

            var result = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!result)
                return Unauthorized(new { message = "Invalid email or password." });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    fullName = user.PersonName,
                    email = user.Email,
                    avatar = user.AvatarPath
                }
            });
        }
        
        private string GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)); // استفاده از یک کلید تصادفی 32 بایتی
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // شناسه یکتا
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "YourIssuerHere",
                audience: "YourAudienceHere",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // زمان انقضای توکن
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token); // تبدیل به رشته
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Ok(new { message = "Logout successful." });
        }
        
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
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

            if (!string.IsNullOrEmpty(request.FullName))
                user.PersonName = request.FullName;

            if (!string.IsNullOrEmpty(request.Password))
            {
                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, request.Password, request.Password);
                if (!passwordChangeResult.Succeeded)
                    return BadRequest(new { message = string.Join(", ", passwordChangeResult.Errors.Select(e => e.Description)) });
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(new { message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) });

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