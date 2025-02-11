namespace Hotel_Core.ServiceContracts
{
    public interface IUserTokenService
    {
        Task AddUserTokenAsync(string userId, string loginProvider, string name, string value);
        Task RemoveUserTokenAsync(string userId, string loginProvider, string name);
        Task<string?> GetUserTokenAsync(string userId, string loginProvider, string name);
        Task<bool> TokenExistsAsync(string userId, string loginProvider, string name);
    }
}