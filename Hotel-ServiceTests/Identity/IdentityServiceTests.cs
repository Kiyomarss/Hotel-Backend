using System.Security.Claims;
using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Hotel_ServiceTests;

public class IdentityServiceTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;
    
    public IdentityServiceTests(IdentityServiceFixture fixture)
    {
        _fixture = fixture;
    }

    #region GetLoggedInUserId

    [Fact]
    public void GetLoggedInUserId_ShouldReturnUserId_WhenUserIsLoggedIn()
    {
        // Arrange
        var userId = "testUserId";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);

        // Act
        var result = _fixture.IdentityService.GetLoggedInUserId();

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetLoggedInUserId_ShouldReturnNull_WhenUserIsNotLoggedIn()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

        // Act
        var result = _fixture.IdentityService.GetLoggedInUserId();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetLoggedInUserId_ShouldReturnNull_WhenUserHasNoNameIdentifierClaim()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // بدون هیچ کلایمی

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);

        // Act
        var result = _fixture.IdentityService.GetLoggedInUserId();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetCurrentUserAsync

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationUser = new ApplicationUser { Id = userId };
    
        // Mocking GetLoggedInUserId to return the test user ID
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] 
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        })));
    
        // Mocking UserManager to return the applicationUser
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(applicationUser);

        // Act
        var result = await _fixture.IdentityService.GetCurrentUserAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldThrowKeyNotFoundException_WhenUserIdIsNotFound()
    {
        // Arrange
        var userId = "testUserId";
    
        // Mocking GetLoggedInUserId to return the test user ID
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] 
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        })));
    
        // Mocking UserManager to return null (user not found)
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.IdentityService.GetCurrentUserAsync());
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldThrowKeyNotFoundException_WhenUserIdIsEmpty()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.IdentityService.GetCurrentUserAsync());
        Assert.Equal("User not found.", exception.Message);
    }

    #endregion

    


}