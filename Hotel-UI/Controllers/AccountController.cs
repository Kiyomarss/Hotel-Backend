using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            return Ok(new { message = "Login successful." });
        }
        
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Ok(new { message = "Logout successful." });
        }
        
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User?.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new
            {
                id = user.Id,
                fullName = user.PersonName,
                email = user.Email
            });
        }
        
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
        {
            var userId = User?.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            // Update user info (password or fullName)
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

            return Ok(new { message = "User updated successfully." });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile avatar)
        {
            var userId = User?.FindFirst("sub")?.Value;

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

            return Ok(new { avatarPath = avatarPath, message = "Avatar updated successfully." });
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