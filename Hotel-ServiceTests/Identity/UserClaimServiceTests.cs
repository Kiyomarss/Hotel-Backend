using System.Security.Claims;
using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.Constant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Hotel_ServiceTests;

public class UserClaimServiceTests : IClassFixture<UserClaimServiceFixture>
{
    private readonly UserClaimServiceFixture _fixture;
    
    public UserClaimServiceTests(UserClaimServiceFixture fixture)
    {
        _fixture = fixture;
    }

    #region AddClaimToUserAsync

    [Fact]
    public async Task AddClaimToUserAsync_ShouldReturnTrue_WhenClaimIsAddedSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claimType = "Permission";
        var claimValue = "Admin";
        var mockUser = _fixture.GetMockUser(userId);

        // Setup the mock for FindByIdAsync and AddClaimAsync
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.AddClaimAsync(mockUser.Object, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _fixture.UserClaimService.AddClaimToUserAsync(userId.ToString(), claimType, claimValue);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AddClaimToUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonExistentUserId";
        var claimType = "Permission";
        var claimValue = "Admin";

        // Setup the mock for FindByIdAsync to return null (user not found)
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _fixture.UserClaimService.AddClaimToUserAsync(userId, claimType, claimValue);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddClaimToUserAsync_ShouldReturnFalse_WhenAddClaimFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claimType = "Permission";
        var claimValue = "Admin";
        var mockUser = _fixture.GetMockUser(userId);

        // Setup the mock for FindByIdAsync and AddClaimAsync
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.AddClaimAsync(mockUser.Object, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _fixture.UserClaimService.AddClaimToUserAsync(userId.ToString(), claimType, claimValue);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region RemoveClaimFromUserAsync

    [Fact]
    public async Task RemoveClaimFromUserAsync_ShouldReturnTrue_WhenClaimIsRemovedSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var claimType = "Permission";
        var claimValue = "SomePermission";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        // Setup the mock for FindByIdAsync and GetClaimsAsync
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(mockUser.Object)).ReturnsAsync(_fixture.GetClaims());
        _fixture.MockUserManager.Setup(x => x.RemoveClaimAsync(mockUser.Object, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _fixture.UserClaimService.RemoveClaimFromUserAsync(userId, claimType, claimValue);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RemoveClaimFromUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonExistentUserId";
        var claimType = "Permission";
        var claimValue = "Admin";

        // Setup the mock for FindByIdAsync to return null (user not found)
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _fixture.UserClaimService.RemoveClaimFromUserAsync(userId, claimType, claimValue);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveClaimFromUserAsync_ShouldReturnFalse_WhenClaimDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var claimType = "Permission";
        var claimValue = "Admin";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        // Setup the mock for FindByIdAsync and GetClaimsAsync
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(mockUser.Object)).ReturnsAsync(new List<Claim>());

        // Act
        var result = await _fixture.UserClaimService.RemoveClaimFromUserAsync(userId, claimType, claimValue);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveClaimFromUserAsync_ShouldReturnFalse_WhenRemoveClaimFails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var claimType = "Permission";
        var claimValue = "Admin";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        // Setup the mock for FindByIdAsync and GetClaimsAsync
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(mockUser.Object)).ReturnsAsync(_fixture.GetClaims());
        _fixture.MockUserManager.Setup(x => x.RemoveClaimAsync(mockUser.Object, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _fixture.UserClaimService.RemoveClaimFromUserAsync(userId, claimType, claimValue);

        // Assert
        Assert.False(result);
    }

    #endregion

}