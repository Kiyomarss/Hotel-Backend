using ContactsManager.Core.Domain.IdentityEntities;

namespace Hotel_Core.DTO.Auth;

public record SignupRequest(string PersonName, string Email, string Password);

public record LoginResult(string Token = "", UserDetails User = null);

public record LoginRequest(string Email, string Password);

public record UserDetails(string FullName,string Email, string? Avatar);

public record UpdateUserRequest(string? FullName, string? Password, string? CurrentPassword);

public record UserDto(string FullName, string Email, string? AvatarPath);