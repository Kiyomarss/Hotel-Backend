using System.Security.Claims;
using ContactsManager.Core.Domain.IdentityEntities;
using FluentAssertions;
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
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(mockUser.Object);

        _fixture.MockUserManager.Setup(x => x.AddClaimAsync(mockUser.Object, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _fixture.UserClaimService.AddClaimToUserAsync(userId.ToString(), claimType, claimValue);

        // Assert
        result.Should().BeTrue(); // Using FluentAssertions
    }

    [Fact]
    public async Task AddClaimToUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonExistentUserId";
        var claimType = "Permission";
        var claimValue = "Admin";

        // Setup the mock for FindByIdAsync to return null (user not found)
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _fixture.UserClaimService.AddClaimToUserAsync(userId, claimType, claimValue);

        // Assert
        result.Should().BeFalse(); // Using FluentAssertions
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
        result.Should().BeFalse(); // Using FluentAssertions
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

        // Mock the user and claims
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        // Setup the mock for FindByIdAsync, GetClaimsAsync, and RemoveClaimAsync
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(_fixture.GetClaims());
        _fixture.MockUserManager.Setup(x => x.RemoveClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()))
                .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _fixture.UserClaimService.RemoveClaimFromUserAsync(userId, claimType, claimValue);

        // Assert
        result.Should().BeTrue();
        _fixture.MockUserManager.Verify(x => x.RemoveClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()), Times.Once);
    }

    [Fact]
    public async Task RemoveClaimFromUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonExistentUserId";
        var claimType = "Permission";
        var claimValue = "Admin";

        // Setup the mock for FindByIdAsync to return null (user not found)
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _fixture.UserClaimService.RemoveClaimFromUserAsync(userId, claimType, claimValue);

        // Assert
        result.Should().BeFalse(); // FluentAssertions to check if the result is false
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
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<Claim>());

        // Act
        var result = await _fixture.UserClaimService.RemoveClaimFromUserAsync(userId, claimType, claimValue);

        // Assert
        result.Should().BeFalse(); // FluentAssertions to check if the result is false
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
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(_fixture.GetClaims());
        _fixture.MockUserManager.Setup(x => x.RemoveClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _fixture.UserClaimService.RemoveClaimFromUserAsync(userId, claimType, claimValue);

        // Assert
        result.Should().BeFalse(); // FluentAssertions to check if the result is false
    }

    #endregion

    #region GetClaimsByUserAsync

    [Fact]
    public async Task GetClaimsByUserAsync_ShouldReturnClaims_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));
        var claims = _fixture.GetClaims();

        // Mock FindByIdAsync and GetClaimsAsync
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(mockUser.Object)).ReturnsAsync(claims);

        // Act
        var result = await _fixture.UserClaimService.GetClaimsByUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(claims.Count, result.Count);
        Assert.All(result, claim => claims.Contains(claim));
    }

    [Fact]
    public async Task GetClaimsByUserAsync_ShouldReturnEmptyList_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Mock FindByIdAsync to return null (user not found)
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _fixture.UserClaimService.GetClaimsByUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region UserHasClaimAsync

    [Fact]
    public async Task UserHasClaimAsync_ShouldReturnTrue_WhenUserHasClaim()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var claimType = "Permission";
        var claimValue = "Admin";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));
        var claims = new List<Claim> { new(claimType, claimValue) };

        // Mock FindByIdAsync and GetClaimsAsync
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(mockUser.Object)).ReturnsAsync(claims);

        // Act
        var result = await _fixture.UserClaimService.UserHasClaimAsync(userId, claimType, claimValue);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserHasClaimAsync_ShouldReturnFalse_WhenUserDoesNotHaveClaim()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var claimType = "Permission";
        var claimValue = "Admin";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));
        var claims = new List<Claim>(); // کاربر هیچ کِلیمی ندارد

        // Mock FindByIdAsync and GetClaimsAsync
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(mockUser.Object)).ReturnsAsync(claims);

        // Act
        var result = await _fixture.UserClaimService.UserHasClaimAsync(userId, claimType, claimValue);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserHasClaimAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var claimType = "Permission";
        var claimValue = "Admin";

        // Mock FindByIdAsync to return null (کاربر پیدا نشد)
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _fixture.UserClaimService.UserHasClaimAsync(userId, claimType, claimValue);

        // Assert
        Assert.False(result);
    }

    #endregion
}