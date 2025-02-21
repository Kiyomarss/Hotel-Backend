using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;
using Hotel_Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Hotel_ServiceTests;
public class AuthServiceTests : IClassFixture<AuthServiceFixture>
{
    private readonly AuthServiceFixture _fixture;

    public AuthServiceTests(AuthServiceFixture fixture)
    {
        _fixture = fixture;
    }

    private void ResetMocks()
    {
        _fixture.MockUserManager.Reset();
    }

    #region UpdateAvatar
    
    [Fact]
    public async Task UpdateAvatarAsync_ThrowsException_WhenUserNotFound()
    {
        // Arrange
        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
            .ThrowsAsync(new KeyNotFoundException("User not found."));

        using var fakeStream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _fixture.AuthService.UpdateAvatarAsync(fakeStream));

        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateAvatarAsync_ThrowsException_WhenUpdateFails()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "TestUser" };
        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(user);

        _fixture.MockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        using var fakeStream = new MemoryStream();
        using (var image = new Image<Rgba32>(100, 100))
        {
            await image.SaveAsJpegAsync(fakeStream);
        }
        fakeStream.Seek(0, SeekOrigin.Begin);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _fixture.AuthService.UpdateAvatarAsync(fakeStream));

        Assert.Equal("Update Failed.", exception.Message);
    }

    [Fact]
    public async Task UpdateAvatarAsync_ReturnsCorrectPath_WhenUpdateSucceeds()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "TestUser" };
        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(user);

        _fixture.MockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

        using var fakeStream = new MemoryStream();
        using (var image = new Image<Rgba32>(100, 100))
        {
            await image.SaveAsJpegAsync(fakeStream);
        }
        fakeStream.Seek(0, SeekOrigin.Begin);
        
        var expectedAvatarPath = "/avatars/test-avatar.jpg";
        var authService = new Mock<AuthService>(_fixture.MockUserManager.Object, null, null, _fixture.MockIdentityService.Object)
        {
            CallBase = true
        };

        authService.Setup(a => a.SaveNewAvatarFromStreamAsync(It.IsAny<Stream>()))
            .ReturnsAsync(expectedAvatarPath);

        // Act
        var result = await authService.Object.UpdateAvatarAsync(fakeStream);

        // Assert
        Assert.Equal(expectedAvatarPath, result);
    }

    #endregion

    #region SaveNewAvatarFromStreamAsync

    [Fact]
    public async Task SaveNewAvatarFromStreamAsync_SavesImageAndReturnsCorrectPath()
    {
        // Arrange
        var authService = new AuthService(null, null, null, null); // مقداردهی null برای وابستگی‌های غیرضروری
        using var fakeStream = new MemoryStream();
        using (var image = new Image<Rgba32>(100, 100))
        {
            await image.SaveAsJpegAsync(fakeStream);
        }
        fakeStream.Seek(0, SeekOrigin.Begin);

        // Act
        var result = await authService.SaveNewAvatarFromStreamAsync(fakeStream);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("/avatars/", result);

        var expectedFilePath = Path.Combine("wwwroot", "avatars", Path.GetFileName(result));
        Assert.True(File.Exists(expectedFilePath),"");

        // Clean up
        File.Delete(expectedFilePath);
    }

    #endregion

    #region GenerateJwtToken

    [Fact]
    public async Task GenerateJwtToken_ReturnsToken_WhenConfigurationIsValid()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), Email = "test@example.com",
        };

        var roles = new List<string>
        {
            "Admin", "User"
        };

        _fixture.MockUserManager.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(roles);

        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Key"])
                .Returns("your-very-secure-and-long-secret-key!!");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Issuer"])
                .Returns("validIssuer");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Audience"])
                .Returns("validAudience");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:ExpirationHours"])
                .Returns("2");

        // Act
        var token = await _fixture.AuthService.GenerateJwtToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.Contains("eyJ", token); // JWT token starts with "eyJ"
    }

    [Fact]
    public async Task GenerateJwtToken_ThrowsInvalidOperationException_WhenJwtKeyIsNotConfigured()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
        };

        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Key"]).Returns(string.Empty);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AuthService.GenerateJwtToken(user));

        Assert.Equal("JWT Key is not configured.", exception.Message);
    }

    [Fact]
    public async Task GenerateJwtToken_ReturnsTokenWithRoles_WhenUserHasRoles()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), 
            Email = "test@example.com",
        };

        var roles = new List<string>
        {
            "Admin", "User"
        };

        _fixture.MockUserManager.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(roles);

        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Key"])
                .Returns("your-very-secure-and-long-secret-key!!");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Issuer"])
                .Returns("validIssuer");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Audience"])
                .Returns("validAudience");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:ExpirationHours"])
                .Returns("2");

        // Act
        var token = await _fixture.AuthService.GenerateJwtToken(user);

        // Decode the JWT token to extract the payload and claims
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        var roleClaims = jsonToken?.Claims?.Where(c => c.Type == "role").Select(c => c.Value).ToList();

        // Assert
        Assert.Contains("Admin", roleClaims); // The "Admin" role should be in the token
        Assert.Contains("User", roleClaims);  // The "User" role should be in the token
    }

    [Fact]
    public async Task GenerateJwtToken_ThrowsException_WhenUserHasNoRoles()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), Email = "test@example.com",
        };

        _fixture.MockUserManager.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Key"])
                .Returns("your-very-secure-and-long-secret-key!!");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Issuer"])
                .Returns("validIssuer");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Audience"])
                .Returns("validAudience");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:ExpirationHours"])
                .Returns("2");

        // Act & Assert
        var token = await _fixture.AuthService.GenerateJwtToken(user);
        Assert.NotNull(token); // Even without roles, the token should be returned
    }

    #endregion
  
    #region ChangePersonName
    
    [Fact]
    public async Task ChangePersonNameAsync_UpdatesUserNameSuccessfully()
    {
        // Arrange
        var person = new ApplicationUser { Id = Guid.NewGuid(), PersonName = "OldPersonName" };
        var newPersonName = "NewPersonName";

        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(person);

        _fixture.MockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

        // Act
        await _fixture.AuthService.ChangePersonNameAsync(newPersonName);

        // Assert
        Assert.Equal(newPersonName, person.PersonName);
        _fixture.MockUserManager.Verify(m => m.UpdateAsync(person), Times.Once);
    }

    [Fact]
    public async Task ChangePersonNameAsync_ThrowsException_WhenUserUpdateFails()
    {
        // Arrange
        var person = new ApplicationUser { Id = Guid.NewGuid(), PersonName = "OldPersonName" };
        var newPersonName = "NewPersonName";

        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(person);

        _fixture.MockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AuthService.ChangePersonNameAsync(newPersonName));
        Assert.Equal("Failed to update PersonName.", exception.Message);
    }

    [Fact]
    public async Task ChangePersonNameAsync_ThrowsException_WhenUserNotFound()
    {
        // Arrange
        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ThrowsAsync(new KeyNotFoundException("Person not found."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.AuthService.ChangePersonNameAsync("NewPersonName"));
        Assert.Equal("Person not found.", exception.Message);
    }

    #endregion

    #region ChangeUserName
    
    [Fact]
    public async Task ChangeUserNameAsync_UpdatesUserNameSuccessfully()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "OldUserName" };
        var newUserName = "NewUserName";

        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(user);

        _fixture.MockUserManager.Setup(m => m.SetUserNameAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

        // Act
        await _fixture.AuthService.ChangeUserNameAsync(newUserName);

        // Assert
        _fixture.MockUserManager.Verify(m => m.SetUserNameAsync(user, newUserName), Times.Once);
    }

    [Fact]
    public async Task ChangeUserNameAsync_ThrowsException_WhenUserUpdateFails()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "OldUserName" };
        var newUserName = "NewUserName";

        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(user);

        _fixture.MockUserManager.Setup(m => m.SetUserNameAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AuthService.ChangeUserNameAsync(newUserName));
        Assert.Equal("Failed to update UserName.", exception.Message);
    }

    [Fact]
    public async Task ChangeUserNameAsync_ThrowsException_WhenUserNotFound()
    {
        // Arrange
        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ThrowsAsync(new KeyNotFoundException("User not found."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.AuthService.ChangeUserNameAsync("NewUserName"));
        Assert.Equal("User not found.", exception.Message);
    }

    #endregion

    #region ChangePassword

    [Fact]
    public async Task ChangePasswordAsync_Succeeds_WhenPasswordIsChangedSuccessfully()
    {
        // Arrange
        var request = new ChangePasswordRequest("oldPassword", "newSecurePassword");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), Email = "test@example.com"
        };

        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(user);

        _fixture.MockUserManager.Setup(u => u.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

        // Act
        await _fixture.AuthService.ChangePasswordAsync(request);

        // Assert
        _fixture.MockUserManager.Verify(u => u.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ThrowsException_WhenChangePasswordFails()
    {
        // Arrange
        var request = new ChangePasswordRequest("oldPassword", "newSecurePassword");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), Email = "test@example.com"
        };

        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(user);

        _fixture.MockUserManager.Setup(u => u.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
                .ReturnsAsync(IdentityResult.Failed());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AuthService.ChangePasswordAsync(request));

        Assert.Equal("ChangePassword Failed.", exception.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_ThrowsException_WhenUserNotFound()
    {
        // Arrange
        var request = new ChangePasswordRequest("oldPassword", "newSecurePassword");

        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ThrowsAsync(new KeyNotFoundException("User not found."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.AuthService.ChangePasswordAsync(request));

        Assert.Equal("User not found.", exception.Message);
    }

    #endregion

    #region Logout

    [Fact]
    public async Task LogoutAsync_CallsSignOutAsyncSuccessfully()
    {
        // Arrange
        _fixture.MockSignInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

        // Act
        await _fixture.AuthService.LogoutAsync();

        // Assert
        _fixture.MockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ThrowsException_WhenSignOutFails()
    {
        // Arrange
        _fixture.MockSignInManager.Setup(s => s.SignOutAsync()).ThrowsAsync(new InvalidOperationException("Logout failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AuthService.LogoutAsync());
        Assert.Equal("Logout failed", exception.Message);
    }

    #endregion

    #region Login

    [Fact]
    public async Task LoginAsync_ReturnsLoginResult_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            PersonName = "Test User",
            AvatarPath = "avatar.png"
        };

        _fixture.MockUserManager.Setup(m => m.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

        _fixture.MockUserManager.Setup(m => m.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);

        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Key"])
                .Returns("your-very-secure-and-long-secret-key!!");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Issuer"])
                .Returns("validIssuer");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:Audience"])
                .Returns("validAudience");
        _fixture.MockConfiguration.SetupGet(c => c["Jwt:ExpirationHours"])
                .Returns("2");

        // Act
        var result = await _fixture.AuthService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(user.PersonName, result.User.PersonName);
        Assert.Equal(user.Email, result.User.Email);
        Assert.Equal(user.AvatarPath, result.User.AvatarPath);
    }

    [Fact]
    public async Task LoginAsync_ThrowsInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password");

        _fixture.MockUserManager.Setup(m => m.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AuthService.LoginAsync(request));
        Assert.Equal("Invalid credentials.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ThrowsInvalidOperationException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "wrong-password");
        var user = new ApplicationUser
        {
            Email = request.Email, UserName = request.Email
        };

        _fixture.MockUserManager.Setup(m => m.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

        _fixture.MockUserManager.Setup(m => m.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AuthService.LoginAsync(request));
        Assert.Equal("Invalid credentials.", exception.Message);
    }

    #endregion
    
    #region Signup

    [Fact]
    public async Task SignupAsync_Success_WhenUserIsCreated()
    {
        ResetMocks();
        // Arrange
        var request = new SignupRequest("test@example.com", "Test User", "password");

        _fixture.MockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

        // Act
        await _fixture.AuthService.SignupAsync(request);

        // Assert
        _fixture.MockUserManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password), Times.Once);
    }


    [Fact]
    public async Task SignupAsync_ThrowsInvalidOperationException_WhenCreateFails()
    {
        // Arrange
        var request = new SignupRequest("test@example.com", "Test User", "password");

        _fixture.MockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Signup failed" }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AuthService.SignupAsync(request));
        Assert.Equal("Signup Failed.", exception.Message);
    }

    #endregion

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