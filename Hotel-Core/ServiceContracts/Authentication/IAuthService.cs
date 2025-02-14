using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;

namespace Hotel_Core.ServiceContracts;

public interface IAuthService
{
    Task<ResultDto<string>> SignupAsync(SignupRequest request);

    Task<ResultDto<LoginResult>> LoginAsync(LoginRequest request);
    
    Task LogoutAsync();
    
    Task<ResultDto<bool>> ChangePasswordAsync(ChangePasswordRequest request);

    Task ChangeUserNameAsync(string newUserName);

    Task ChangePersonNameAsync(string newPersonName);
    
    Task<string> UpdateAvatarAsync(Stream avatarStream);

    Task DeleteUserAsync(string userId);
}