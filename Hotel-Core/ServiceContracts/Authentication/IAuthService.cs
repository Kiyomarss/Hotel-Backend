using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;

namespace Hotel_Core.ServiceContracts;

public interface IAuthService
{
    Task<ResultDto<string>> SignupAsync(SignupRequest request);

    Task<ResultDto<LoginResult>> LoginAsync(LoginRequest request);
    
    Task LogoutAsync();
    
    Task<ResultDto<UserDto>> UpdateUserAsync(UpdateUserRequest request);
    
    Task<ResultDto<string>> UpdateAvatarAsync(Stream avatarStream);

    Task<ResultDto<bool>> DeleteUserAsync(string userId);
}