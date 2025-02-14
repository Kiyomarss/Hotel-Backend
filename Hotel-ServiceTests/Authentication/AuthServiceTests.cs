using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Hotel_ServiceTests
{
public class AuthServiceTests : IClassFixture<AuthServiceFixture>
{
    private readonly AuthServiceFixture _fixture;

    public AuthServiceTests(AuthServiceFixture fixture)
    {
        _fixture = fixture;
    }

    #region DeleteUser

    [Fact]
    public async Task DeleteUserAsync_Success_WhenUserExists()
    {
        // Arrange
        const string userId = "valid-user-id";
        var user = new ApplicationUser { Id = Guid.NewGuid() };

        _fixture.MockIdentityService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

        _fixture.MockUserManager.Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

        // Act
        await _fixture.AuthService.DeleteUserAsync(userId);

        // Assert
        _fixture.MockUserManager.Verify(m => m.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ThrowsKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        const string userId = "nonexistent-user-id";

        _fixture.MockIdentityService.Setup(s => s.GetUserByIdAsync(userId))
                .ThrowsAsync(new KeyNotFoundException("User not found."));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.AuthService.DeleteUserAsync(userId));
    }

    [Fact]
    public async Task DeleteUserAsync_ThrowsInvalidOperationException_WhenDeleteFails()
    {
        // Arrange
        const string userId = "valid-user-id";
        var user = new ApplicationUser { Id = Guid.NewGuid() };

        _fixture.MockIdentityService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

        _fixture.MockUserManager.Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Delete failed" }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AuthService.DeleteUserAsync(userId));
        Assert.Equal("Delete Failed.", exception.Message);
    }

    #endregion
}
}
