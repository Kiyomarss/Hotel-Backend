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
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(ResultDto<string>.Success("Logout successful."));
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> UpdateCurrentUser(UpdateUserRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ResultDto<string>.Failure("User is not authenticated."));

            var result = await _authService.UpdateUserAsync(userId, request);
            
            if (!result.IsSuccess)
                return BadRequest(ResultDto<string>.Failure(result.Message));

            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/octet-stream")]
        public async Task<IActionResult> UpdateAvatar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ResultDto<string>.Failure("User is not authenticated."));

            if (Request.ContentLength is null or 0)
                return BadRequest(ResultDto<string>.Failure("No avatar file provided."));

            var result = await _authService.UpdateAvatarAsync(userId, Request.Body);
            
            if (!result.IsSuccess)
                return BadRequest(ResultDto<string>.Failure(result.Message));

            return Ok(result);
        }
    }
}