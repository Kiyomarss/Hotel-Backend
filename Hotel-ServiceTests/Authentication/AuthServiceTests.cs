using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

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

        _fixture.MockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

        // Act
        await _fixture.AuthService.ChangeUserNameAsync(newUserName);

        // Assert
        Assert.Equal(newUserName, user.UserName);
        _fixture.MockUserManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangeUserNameAsync_ThrowsException_WhenUserUpdateFails()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "OldUserName" };
        var newUserName = "NewUserName";

        _fixture.MockIdentityService.Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(user);

        _fixture.MockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
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

        var token = await _fixture.AuthService.GenerateJwtToken(user);
        Assert.NotNull(token);


        // Act
        var result = await _fixture.AuthService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        ;
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