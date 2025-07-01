using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Hotel_Core.ServiceContracts;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;
using Microsoft.AspNetCore.Authorization;

namespace Hotel_UI.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupRequest request)
        {
            await _authService.SignupAsync(request);

            return Ok(new MessageResponse("Signup successful."));
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();

            return Ok(new MessageResponse("Logout successful."));
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            await _authService.ChangePasswordAsync(request);

            return Ok(new MessageResponse("Change password successfully."));
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> UpdatePersonName(UpdatePersonNameRequest nameRequest)
        {
            await _authService.UpdatePersonNameAsync(nameRequest.NewPersonName);
        
            return Ok(new MessageResponse("PersonName updated successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAvatar([FromForm] UpdateAvatarRequest request)
        {
            if (request.Avatar.Length == 0)
                return BadRequest(new MessageResponse("No avatar file provided."));

            await using var stream = request.Avatar.OpenReadStream();

            var result = await _authService.UpdateAvatarAsync(stream);

            return Ok(result);
        }
        
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            await _authService.DeleteUserAsync(userId);

            return Ok(new MessageResponse("User deleted successfully."));
        }
    }
}