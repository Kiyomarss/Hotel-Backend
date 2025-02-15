using Moq;
using Microsoft.AspNetCore.Mvc;
using Hotel_Core.ServiceContracts;
using Hotel_UI.Controllers;
using Hotel_Core.DTO;
using Hotel_Core.DTO.Auth;

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
