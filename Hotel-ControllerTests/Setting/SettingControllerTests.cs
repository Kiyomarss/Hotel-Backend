using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Hotel_UI.Controllers;
using ServiceContracts;
using Entities;
using Hotel_Core.DTO.Setting;

public class SettingsControllerTests
{
    private readonly Mock<ISettingGetterService> _mockGetterService;
    private readonly Mock<ISettingUpdaterService> _mockUpdaterService;
    private readonly SettingsController _controller;

    public SettingsControllerTests()
    {
        _mockGetterService = new Mock<ISettingGetterService>();
        _mockUpdaterService = new Mock<ISettingUpdaterService>();
        _controller = new SettingsController(_mockGetterService.Object, _mockUpdaterService.Object);
    }

    [Fact]
    public async Task Edit_ReturnsOk_WithUpdatedSetting()
    {
        // Arrange
        var settingDto = new Setting { MaxGuestsPerBooking = 4, BreakfastPrice = 10, MinBookingLength = 2, MaxBookingLength = 7 };

        _mockUpdaterService
            .Setup(s => s.UpdateSetting(settingDto))
            .ReturnsAsync(settingDto);

        // Act
        var result = await _controller.Edit(settingDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<SettingResponseDto>(okResult.Value);

        Assert.Equal("Setting updated successfully", response.Message);
        Assert.Equal(settingDto.MaxGuestsPerBooking, response.Setting.MaxGuestsPerBooking);
        Assert.Equal(settingDto.BreakfastPrice, response.Setting.BreakfastPrice);
        Assert.Equal(settingDto.MinBookingLength, response.Setting.MinBookingLength);
        Assert.Equal(settingDto.MaxBookingLength, response.Setting.MaxBookingLength);
    }

    [Fact]
    public async Task GetSettings_ReturnsOk_WithCorrectSetting()
    {
        // Arrange
        var setting = new Setting { MaxGuestsPerBooking = 5, BreakfastPrice = 15, MinBookingLength = 3, MaxBookingLength = 10 };

        _mockGetterService
            .Setup(s => s.GetSetting())
            .ReturnsAsync(setting);

        // Act
        var result = await _controller.GetSettings();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<SettingResponseDto>(okResult.Value);

        Assert.Equal(setting.MaxGuestsPerBooking, response.Setting.MaxGuestsPerBooking);
        Assert.Equal(setting.BreakfastPrice, response.Setting.BreakfastPrice);
        Assert.Equal(setting.MinBookingLength, response.Setting.MinBookingLength);
        Assert.Equal(setting.MaxBookingLength, response.Setting.MaxBookingLength);
    }
}
