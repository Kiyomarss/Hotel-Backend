using System.Security.Claims;
using ContactsManager.Core.Domain.IdentityEntities;
using FluentAssertions;
using Hotel_Core.Constant;
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

        // Assert;
        result.Should().Be(userId);
    }

    [Fact]
    public void GetLoggedInUserId_ShouldReturnNull_WhenUserIsNotLoggedIn()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

        // Act
        var result = _fixture.IdentityService.GetLoggedInUserId();

        // Assert
        result.Should().BeNull();
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
        result.Should().BeNull();
    }

    #endregion

    #region IsUserLoggedIn

    [Fact]
    public void IsUserLoggedIn_ShouldReturnTrue_WhenUserIsLoggedIn()
    {
        // Arrange
        var userId = "testUserId";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);

        // Act
        var result = _fixture.IdentityService.IsUserLoggedIn();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserLoggedIn_ShouldReturnFalse_WhenUserIsNotLoggedIn()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns((ClaimsPrincipal)null);

        // Act
        var result = _fixture.IdentityService.IsUserLoggedIn();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetCurrentUserAsync

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUser = _fixture.GetMockUser(userId);
    
        // Mocking GetLoggedInUserId to return the test user ID
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] 
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        })));
    
        // Mocking UserManager to return the applicationUser
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(mockUser.Object);

        // Act
        var result = await _fixture.IdentityService.GetCurrentUserAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
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

        // Act
        var act = async () => await _fixture.IdentityService.GetCurrentUserAsync();

        // Assert
        await act.Should()
                 .ThrowAsync<KeyNotFoundException>()
                 .WithMessage("User not found.");
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldThrowKeyNotFoundException_WhenUserIdIsEmpty()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        // Act
        var act = async () => await _fixture.IdentityService.GetCurrentUserAsync();

        // Assert
        await act.Should()
                 .ThrowAsync<KeyNotFoundException>()
                 .WithMessage("User not found.");
    }

    #endregion

    #region GetCurrentUserWithoutErrorAsync

    [Fact]
    public async Task GetCurrentUserWithoutErrorAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] 
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(mockUser.Object);

        // Act
        var result = await _fixture.IdentityService.GetCurrentUserWithoutErrorAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(mockUser.Object.Id);
    }


    [Fact]
    public async Task GetCurrentUserWithoutErrorAsync_ShouldReturnNull_WhenUserIsNotLoggedIn()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

        // Act
        var result = await _fixture.IdentityService.GetCurrentUserWithoutErrorAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserWithoutErrorAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "testUserId";

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _fixture.IdentityService.GetCurrentUserWithoutErrorAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserByIdAsync

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(mockUser.Object);

        // Act
        var result = await _fixture.IdentityService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(mockUser.Object.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetUserByIdAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty(string userId)
    {
        // Act & Assert
        await FluentActions.Awaiting(() => _fixture.IdentityService.GetUserByIdAsync(userId))
                           .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonExistingUserId";

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        await FluentActions.Awaiting(() => _fixture.IdentityService.GetUserByIdAsync(userId))
                           .Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region CurrentUserHasAnyRoleAsync

    [Fact]
    public async Task CurrentUserHasAnyRoleAsync_ShouldReturnTrue_WhenUserHasAtLeastOneRole()
    {
        // Arrange
        var roles = new List<string>
        {
            "Admin", "User"
        };
        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(mockUser.Object);

        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(mockUser.Object))
                .ReturnsAsync(roles);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAnyRoleAsync("Admin", "Manager");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CurrentUserHasAnyRoleAsync_ShouldReturnFalse_WhenUserHasNoMatchingRole()
    {
        // Arrange
        var roles = new List<string>
        {
            "User"
        };
        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(mockUser.Object);

        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(mockUser.Object))
                .ReturnsAsync(roles);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAnyRoleAsync("Admin", "Manager");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CurrentUserHasAnyRoleAsync_ShouldReturnFalse_WhenUserIsNotLoggedIn()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns((ClaimsPrincipal)null);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAnyRoleAsync("Admin");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CurrentUserHasAnyRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "testUserId";

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAnyRoleAsync("Admin");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CurrentUserHasRoleAsync

    [Fact]
    public async Task CurrentUserHasRoleAsync_ShouldReturnTrue_WhenUserHasRole()
    {
        // Arrange
        var roleName = "Admin";
        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(mockUser.Object);

        _fixture.MockUserManager.Setup(x => x.IsInRoleAsync(mockUser.Object, roleName))
                .ReturnsAsync(true);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasRoleAsync(roleName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CurrentUserHasRoleAsync_ShouldReturnFalse_WhenUserDoesNotHaveRole()
    {
        // Arrange
        var roleName = "Admin";
        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(mockUser.Object);

        _fixture.MockUserManager.Setup(x => x.IsInRoleAsync(mockUser.Object, roleName))
                .ReturnsAsync(false);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasRoleAsync(roleName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CurrentUserHasRoleAsync_ShouldReturnFalse_WhenUserIsNotLoggedIn()
    {
        // Arrange
        var roleName = "Admin";

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns((ClaimsPrincipal)null);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasRoleAsync(roleName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CurrentUserHasRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "testUserId";
        var roleName = "Admin";

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasRoleAsync(roleName);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CurrentUserHasAllRolesAsync

    [Fact]
    public async Task CurrentUserHasAllRolesAsync_ShouldReturnTrue_WhenUserHasAllRoles()
    {
        // Arrange
        var roles = new List<string>
        {
            "Admin", "User", "Manager"
        };
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1cfb4096-1f1c-4381-897c-08dd348ace72")
        })));

        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));
        
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(mockUser.Object)).ReturnsAsync(roles);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAllRolesAsync("Admin", "User");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CurrentUserHasAllRolesAsync_ShouldReturnFalse_WhenUserLacksSomeRoles()
    {
        // Arrange
        var roles = new List<string>
        {
            "Admin", "User"
        };
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "testUserId")
        })));

        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));
        
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(mockUser.Object)).ReturnsAsync(roles);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAllRolesAsync("Admin", "Manager");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CurrentUserHasAllRolesAsync_ShouldReturnFalse_WhenUserHasNoRoles()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "testUserId")
        })));

        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));
        
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(mockUser.Object)).ReturnsAsync(new List<string>());

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAllRolesAsync("Admin");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasAccessAsync

    [Fact]
    public async Task HasAccessAsync_ShouldReturnTrue_WhenUserHasFullAccessClaim()
    {
        // Arrange
        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));
        var claims = new List<Claim>
        {
            new(Constant.Claims.FullAccess, "true")
        };
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        })));
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(mockUser.Object)).ReturnsAsync(claims);

        // Act
        var result = await _fixture.IdentityService.HasAccessAsync("SomePermission");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAccessAsync_ShouldReturnTrue_WhenUserHasRequiredPermissionClaim()
    {
        // Arrange
        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));
        var claims = new List<Claim>
        {
            new("Permission", "SomePermission") // Permission claim
        };
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        })));
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(mockUser.Object)).ReturnsAsync(claims);

        // Act
        var result = await _fixture.IdentityService.HasAccessAsync("SomePermission");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAccessAsync_ShouldReturnFalse_WhenUserLacksAccessClaim()
    {
        // Arrange
        var userId = "1cfb4096-1f1c-4381-897c-08dd348ace72";
        var mockUser = _fixture.GetMockUser(Guid.Parse(userId));
        var claims = new List<Claim>
        {
            new("Permission", "OtherPermission") // Some permission other than the required one
        };
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        })));
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(mockUser.Object);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(mockUser.Object)).ReturnsAsync(claims);

        // Act
        var result = await _fixture.IdentityService.HasAccessAsync("SomePermission");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAccessAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal()); // No user logged in

        // Act
        var result = await _fixture.IdentityService.HasAccessAsync("SomePermission");

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}