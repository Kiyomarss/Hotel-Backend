using Moq;
using Microsoft.AspNetCore.Mvc;
using Hotel_Core.DTO;
using Hotel_Core.ServiceContracts;
using Hotel_UI.Controllers;

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

        [Fact]
        public async Task DeleteUser_ReturnsBadRequest_WhenUserNotFound()
        {
            var userId = "nonexistent-user-id";
            _mockAuthService.Setup(service => service.DeleteUserAsync(userId))
                            .ReturnsAsync(ResultDto<bool>.Failure("User not found."));

            var result = await _controller.DeleteUser(userId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("User not found.", response.Message);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenUserDeletedSuccessfully()
        {
            var userId = "valid-user-id";
            _mockAuthService.Setup(service => service.DeleteUserAsync(userId))
                            .ReturnsAsync(ResultDto<bool>.Success(true, "User deleted successfully."));

            var result = await _controller.DeleteUser(userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("User deleted successfully.", response.Message);
        }
    }
}