using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;
using Microsoft.AspNetCore.Identity;

namespace Hotel_Core.ServiceContracts;

public interface IAuthService
{
    Task SignupAsync(SignupRequest request);

    Task<LoginResult> LoginAsync(LoginRequest request);
    
    Task LogoutAsync();
    
    Task ChangePasswordAsync(ChangePasswordRequest request);

    Task ChangeUserNameAsync(string newUserName);

    Task ChangePersonNameAsync(string newPersonName);
    
    Task<string> UpdateAvatarAsync(Stream avatarStream);

    Task DeleteUserAsync(string userId);
    
    Task<ApplicationUser?> GetUserByExternalProviderAsync(UserLoginInfo loginInfo);
    
    Task<ApplicationUser?> RegisterUserFromExternalProviderAsync(ExternalLoginInfo info);
}