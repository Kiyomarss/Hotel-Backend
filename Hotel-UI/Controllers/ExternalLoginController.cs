using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;
using Hotel_Core.ServiceContracts;
using Hotel_Core.ServiceContracts.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_UI.Controllers;

public class ExternalLoginController : BaseController
{
    private readonly IUserLoginService _userLoginService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public ExternalLoginController(IUserLoginService userLoginService, UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userLoginService = userLoginService;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddExternalLogin([FromBody] AddExternalLoginRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
            return NotFound("User not found.");

        var loginInfo = new UserLoginInfo(request.LoginProvider, request.ProviderKey, request.ProviderDisplayName);

        var result = await _userLoginService.AddLoginAsync(user, loginInfo);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("External login added successfully.");
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveExternalLogin([FromBody] RemoveExternalLoginRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
            return NotFound("User not found.");

        var result = await _userLoginService.RemoveLoginAsync(user, request.LoginProvider, request.ProviderKey);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("External login removed successfully.");
    }

    [HttpGet("logins/{userId}")]
    public async Task<IActionResult> GetExternalLogins(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound("User not found.");

        var logins = await _userLoginService.GetLoginsAsync(user);

        return Ok(logins);
    }
    
    [HttpPost("external-login")]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginRequest request)
    {
        var loginInfo = new UserLoginInfo(request.LoginProvider, request.ProviderKey, request.ProviderDisplayName);

        var user = await _userLoginService.ExternalLoginAsync(loginInfo);

        if (user == null)
            return Unauthorized("External login failed.");

        var token = await _tokenService.GenerateJwtToken(user);
        return Ok(new LoginResult(token));
    }
}