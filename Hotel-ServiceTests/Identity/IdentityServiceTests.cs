using System.Security.Claims;
using ContactsManager.Core.Domain.IdentityEntities;
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
        Assert.True(result);
    }

    [Fact]
    public void IsUserLoggedIn_ShouldReturnFalse_WhenUserIsNotLoggedIn()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns((ClaimsPrincipal)null);

        // Act
        var result = _fixture.IdentityService.IsUserLoggedIn();

        // Assert
        Assert.False(result);
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

    #region GetCurrentUserWithoutErrorAsync

    [Fact]
    public async Task GetCurrentUserWithoutErrorAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = "testUserId";
        var applicationUser = new ApplicationUser { Id = Guid.NewGuid() };

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(applicationUser);

        // Act
        var result = await _fixture.IdentityService.GetCurrentUserWithoutErrorAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(applicationUser, result);
    }

    [Fact]
    public async Task GetCurrentUserWithoutErrorAsync_ShouldReturnNull_WhenUserIsNotLoggedIn()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

        // Act
        var result = await _fixture.IdentityService.GetCurrentUserWithoutErrorAsync();

        // Assert
        Assert.Null(result);
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
        Assert.Null(result);
    }

    #endregion

    #region GetUserByIdAsync

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = "testUserId";
        var applicationUser = new ApplicationUser { Id = Guid.NewGuid() };

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(applicationUser);

        // Act
        var result = await _fixture.IdentityService.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(applicationUser, result);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _fixture.IdentityService.GetUserByIdAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => _fixture.IdentityService.GetUserByIdAsync(""));
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonExistingUserId";

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.IdentityService.GetUserByIdAsync(userId));
    }
    
    #endregion

    #region CurrentUserHasAnyRoleAsync

    [Fact]
    public async Task CurrentUserHasAnyRoleAsync_ShouldReturnTrue_WhenUserHasAtLeastOneRole()
    {
        // Arrange
        var userId = "testUserId";
        var roles = new List<string>
        {
            "Admin", "User"
        };
        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid()
        };

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(applicationUser);

        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(applicationUser))
                .ReturnsAsync(roles);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAnyRoleAsync("Admin", "Manager");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CurrentUserHasAnyRoleAsync_ShouldReturnFalse_WhenUserHasNoMatchingRole()
    {
        // Arrange
        var userId = "testUserId";
        var roles = new List<string>
        {
            "User"
        };
        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid()
        };

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(applicationUser);

        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(applicationUser))
                .ReturnsAsync(roles);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAnyRoleAsync("Admin", "Manager");

        // Assert
        Assert.False(result);
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
        Assert.False(result);
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
        Assert.False(result);
    }

    #endregion

    #region CurrentUserHasRoleAsync

    [Fact]
    public async Task CurrentUserHasRoleAsync_ShouldReturnTrue_WhenUserHasRole()
    {
        // Arrange
        var userId = "testUserId";
        var roleName = "Admin";
        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid()
        };

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(applicationUser);

        _fixture.MockUserManager.Setup(x => x.IsInRoleAsync(applicationUser, roleName))
                .ReturnsAsync(true);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasRoleAsync(roleName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CurrentUserHasRoleAsync_ShouldReturnFalse_WhenUserDoesNotHaveRole()
    {
        // Arrange
        var userId = "testUserId";
        var roleName = "Admin";
        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid()
        };

        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                })));

        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(applicationUser);

        _fixture.MockUserManager.Setup(x => x.IsInRoleAsync(applicationUser, roleName))
                .ReturnsAsync(false);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasRoleAsync(roleName);

        // Assert
        Assert.False(result);
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
        Assert.False(result);
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
        Assert.False(result);
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
            new Claim(ClaimTypes.NameIdentifier, "testUserId")
        })));

        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid()
        };
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync("testUserId")).ReturnsAsync(applicationUser);
        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(applicationUser)).ReturnsAsync(roles);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAllRolesAsync("Admin", "User");

        // Assert
        Assert.True(result);
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

        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid()
        };
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync("testUserId")).ReturnsAsync(applicationUser);
        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(applicationUser)).ReturnsAsync(roles);

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAllRolesAsync("Admin", "Manager");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CurrentUserHasAllRolesAsync_ShouldReturnFalse_WhenUserHasNoRoles()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "testUserId")
        })));

        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid()
        };
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync("testUserId")).ReturnsAsync(applicationUser);
        _fixture.MockUserManager.Setup(x => x.GetRolesAsync(applicationUser)).ReturnsAsync(new List<string>());

        // Act
        var result = await _fixture.IdentityService.CurrentUserHasAllRolesAsync("Admin");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region HasAccessAsync

    [Fact]
    public async Task HasAccessAsync_ShouldReturnTrue_WhenUserHasFullAccessClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationUser = new ApplicationUser
        {
            Id = userId
        };
        var claims = new List<Claim>
        {
            new(Constant.Claims.FullAccess, "true")
        };
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        })));
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(applicationUser);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(applicationUser)).ReturnsAsync(claims);

        // Act
        var result = await _fixture.IdentityService.HasAccessAsync("SomePermission");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAccessAsync_ShouldReturnTrue_WhenUserHasRequiredPermissionClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationUser = new ApplicationUser
        {
            Id = userId
        };
        var claims = new List<Claim>
        {
            new("Permission", "SomePermission") // Permission claim
        };
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        })));
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(applicationUser);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(applicationUser)).ReturnsAsync(claims);

        // Act
        var result = await _fixture.IdentityService.HasAccessAsync("SomePermission");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAccessAsync_ShouldReturnFalse_WhenUserLacksAccessClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationUser = new ApplicationUser
        {
            Id = userId
        };
        var claims = new List<Claim>
        {
            new("Permission", "OtherPermission") // Some permission other than the required one
        };
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        })));
        _fixture.MockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(applicationUser);
        _fixture.MockUserManager.Setup(x => x.GetClaimsAsync(applicationUser)).ReturnsAsync(claims);

        // Act
        var result = await _fixture.IdentityService.HasAccessAsync("SomePermission");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasAccessAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        _fixture.MockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal()); // No user logged in

        // Act
        var result = await _fixture.IdentityService.HasAccessAsync("SomePermission");

        // Assert
        Assert.False(result);
    }

    #endregion
}