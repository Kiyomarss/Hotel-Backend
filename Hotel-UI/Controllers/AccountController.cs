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

            return result.IsSuccess ? Ok(new MessageResponse(result.Message)) : BadRequest(new MessageResponse(result.Message));
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            return result.IsSuccess ? Ok(new DataResponse<LoginResult>(result.Data)) : Unauthorized(new MessageResponse(result.Message));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(new DataResponse<string>("Logout successful."));
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> UpdateCurrentUser(UpdateUserRequest request)
        {
            var result = await _authService.UpdateUserAsync(request);

            return result.IsSuccess ? Ok(new DataResponse<UserDto>(result.Data)) : BadRequest(new MessageResponse(result.Message));
        }

        [HttpPost]
        [Consumes("application/octet-stream")]
        public async Task<IActionResult> UpdateAvatar()
        {
            if (Request.ContentLength is null or 0)
                return BadRequest(new MessageResponse("No avatar file provided."));

            var result = await _authService.UpdateAvatarAsync(Request.Body);

            return result.IsSuccess ? Ok(new DataResponse<string>(result.Data)) : BadRequest(new MessageResponse(result.Message));
        }
        
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _authService.DeleteUserAsync(userId);

            return result.IsSuccess ? Ok(new MessageResponse(result.Message)) : BadRequest(new MessageResponse(result.Message));
        }
    }
}