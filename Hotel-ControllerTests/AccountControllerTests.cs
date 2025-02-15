using Moq;
using Microsoft.AspNetCore.Mvc;
using Hotel_Core.ServiceContracts;
using Hotel_UI.Controllers;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;
using Microsoft.AspNetCore.Http;

namespace Hotel_ControllerTests
{
    public class AccountControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AccountController(_mockAuthService.Object);
        }

        #region ChangePassword

        [Fact]
        public async Task ChangePassword_ReturnsOk_WhenPasswordChangesSuccessfully()
        {
            // Arrange
            var request = new ChangePasswordRequest("oldPassword", "newSecurePassword");

            _mockAuthService.Setup(s => s.ChangePasswordAsync(request))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Change password successfully.", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ThrowsException_WhenChangePasswordFails()
        {
            // Arrange
            var request = new ChangePasswordRequest("oldPassword", "newSecurePassword");

            _mockAuthService.Setup(s => s.ChangePasswordAsync(request))
                            .ThrowsAsync(new InvalidOperationException("ChangePassword Failed."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.ChangePassword(request));
        }

        #endregion

        #region Logout

        [Fact]
        public async Task Logout_ReturnsOkResult_WhenLogoutIsSuccessful()
        {
            // Arrange
            _mockAuthService.Setup(service => service.LogoutAsync())
                            .Returns(Task.CompletedTask);
            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var messageResponse = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Logout successful.", messageResponse.Message);
        }

        [Fact]
        public async Task Logout_ReturnsInternalServerError_WhenLogoutFails()
        {
            // Arrange
            _mockAuthService.Setup(service => service.LogoutAsync())
                            .ThrowsAsync(new Exception("Logout failed"));

            // Act
            var result = await _controller.Logout();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            var messageResponse = Assert.IsType<MessageResponse>(statusCodeResult.Value);
            Assert.Equal("Logout failed", messageResponse.Message);
        }

        #endregion
        
        #region Login

        [Fact]
        public async Task Login_ReturnsOk_WhenLoginIsSuccessful()
        {
            // Arrange
            var request = new LoginRequest("test@example.com", "password");
            var expectedResult = new LoginResult("mocked-token", new UserDetails("Test User", "test@example.com", "avatar.png"));

            _mockAuthService.Setup(s => s.LoginAsync(request))
                            .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResult>(okResult.Value);
            Assert.Equal("mocked-token", response.Token);
            Assert.Equal("Test User", response.User.PersonName);
            Assert.Equal("test@example.com", response.User.Email);
            Assert.Equal("avatar.png", response.User.AvatarPath);
        }

        [Fact]
        public async Task Login_ThrowsInvalidOperationException_WhenCredentialsAreInvalid()
        {
            // Arrange
            var request = new LoginRequest("test@example.com", "wrongpassword");

            _mockAuthService.Setup(s => s.LoginAsync(request))
                            .ThrowsAsync(new InvalidOperationException("Invalid credentials."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Login(request));
            Assert.Equal("Invalid credentials.", exception.Message);
        }

        #endregion

        #region Signup

        [Fact]
        public async Task Signup_ReturnsOk_WhenSignupIsSuccessful()
        {
            // Arrange
            var request = new SignupRequest("test@example.com", "Test User", "password");

            _mockAuthService.Setup(s => s.SignupAsync(request))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Signup(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Signup successful.", response.Message);
        }

        [Fact]
        public async Task Signup_ThrowsException_WhenSignupFails()
        {
            // Arrange
            var request = new SignupRequest("test@example.com", "Test User", "password");

            _mockAuthService.Setup(s => s.SignupAsync(request))
                            .ThrowsAsync(new InvalidOperationException("Signup failed."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Signup(request));
            Assert.Equal("Signup failed.", exception.Message);
        }

        #endregion

        #region DeleteUser Tests

        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenUserDeletedSuccessfully()
        {
            // Arrange
            var userId = "valid-user-id";
            _mockAuthService.Setup(service => service.DeleteUserAsync(userId))
                            .Returns(Task.CompletedTask); // حذف موفق

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("User deleted successfully.", response.Message);
        }

        [Fact]
        public async Task DeleteUser_ThrowsKeyNotFoundException_WhenUserNotFound()
        {
            // Arrange
            var userId = "nonexistent-user-id";
            _mockAuthService.Setup(service => service.DeleteUserAsync(userId))
                            .ThrowsAsync(new KeyNotFoundException("User not found.")); // شبیه‌سازی خطای نبود کاربر

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteUser(userId));
        }

        [Fact]
        public async Task DeleteUser_ThrowsInvalidOperationException_WhenDeletionFails()
        {
            // Arrange
            var userId = "valid-user-id";
            _mockAuthService.Setup(service => service.DeleteUserAsync(userId))
                            .ThrowsAsync(new InvalidOperationException("Delete Failed.")); // شبیه‌سازی خطای حذف ناموفق

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteUser(userId));
        }

        #endregion
    }
}
