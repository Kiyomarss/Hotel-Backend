using ContactsManager.Core.Domain.IdentityEntities;

namespace Hotel_Core.ServiceContracts.Base;

public interface ITokenService
{
    Task<string> GenerateJwtToken(ApplicationUser user);
}