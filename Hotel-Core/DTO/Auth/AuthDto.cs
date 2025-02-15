using ContactsManager.Core.Domain.IdentityEntities;

namespace Hotel_Core.DTO.Auth;

public record SignupRequest(string PersonName, string Email, string Password);

public record LoginResult(string Token = "", UserDetails User = null);

public record LoginRequest(string Email, string Password);

public record UserDetails(string? PersonName,string Email, string? AvatarPath);

public record ChangePasswordRequest(string CurrentPassword, string newPassword);

public record UpdateUserNameRequest(string PersonName);

public record UserDto(string FullName, string Email, string? AvatarPath);