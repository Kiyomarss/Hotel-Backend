using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Http;

namespace Hotel_Core.DTO.Auth;

public record SignupRequest(string PersonName, string Email, string Password);

public record LoginResult(string Token = "", UserDetails User = null, string RefreshToken = "");

public record LoginRequest(string Email, string Password);

public record UserDetails(string? PersonName,string Email, string? AvatarPath);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record UpdateAvatarRequest(IFormFile Avatar);

public record UpdatePersonNameRequest(string NewPersonName);

public record UserDto(string FullName, string Email, string? AvatarPath);