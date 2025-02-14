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
            var result = await _authService.SignupAsync(request);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
    
            if (!result.IsSuccess)
                return Unauthorized(ResultDto<string>.Failure(result.Message));

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(ResultDto<string>.Success("Logout successful."));
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> UpdateCurrentUser(UpdateUserRequest request)
        {
            var result = await _authService.UpdateUserAsync(request);
            
            if (!result.IsSuccess)
                return BadRequest(ResultDto<string>.Failure(result.Message));

            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/octet-stream")]
        public async Task<IActionResult> UpdateAvatar()
        {
            if (Request.ContentLength is null or 0)
                return BadRequest(ResultDto<string>.Failure("No avatar file provided."));

            var result = await _authService.UpdateAvatarAsync(Request.Body);
            
            if (!result.IsSuccess)
                return BadRequest(ResultDto<string>.Failure(result.Message));

            return Ok(result);
        }
        
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _authService.DeleteUserAsync(userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new MessageResponse(result.Message));
            }

            return Ok(new MessageResponse(result.Message));
        }
    }
}