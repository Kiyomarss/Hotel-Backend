﻿using Moq;
using FluentAssertions;
using AutoFixture;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using Services;
using Entities;
using Hotel_Core.DTO;

public class SettingGetterServiceTests
{
    private readonly Mock<ISettingRepository> _mockSettingRepository;
    private readonly Mock<ILogger<SettingGetterService>> _mockLogger;
    private readonly SettingGetterService _settingGetterService;
    private readonly Fixture _fixture;

    public SettingGetterServiceTests()
    {
        _mockSettingRepository = new Mock<ISettingRepository>();
        _mockLogger = new Mock<ILogger<SettingGetterService>>();
        _settingGetterService = new SettingGetterService(_mockSettingRepository.Object, _mockLogger.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetSetting_ShouldReturnSettingResponse_WhenSettingExists()
    {
        // Arrange
        var setting = _fixture.Build<Setting>()
            .With(s => s.MinBookingLength, 2)
            .With(s => s.MaxBookingLength, 7)
            .With(s => s.MaxGuestsPerBooking, 4)
            .With(s => s.BreakfastPrice, 20.5)
            .Create();

        var settingResponse = new SettingResponse
        {
            MinBookingLength = setting.MinBookingLength,
            MaxBookingLength = setting.MaxBookingLength,
            MaxGuestsPerBooking = setting.MaxGuestsPerBooking,
            BreakfastPrice = setting.BreakfastPrice
        };

        _mockSettingRepository
            .Setup(repo => repo.GetSetting())
            .ReturnsAsync(setting);

        // Act
        var result = await _settingGetterService.GetSetting();

        // Assert
        result.Should().NotBeNull();
        result.MinBookingLength.Should().Be(settingResponse.MinBookingLength);
        result.MaxBookingLength.Should().Be(settingResponse.MaxBookingLength);
        result.MaxGuestsPerBooking.Should().Be(settingResponse.MaxGuestsPerBooking);
        result.BreakfastPrice.Should().Be(settingResponse.BreakfastPrice);
    }

    [Fact]
    public async Task GetSetting_ShouldThrowInvalidOperationException_WhenSettingDoesNotExist()
    {
        // Arrange
        _mockSettingRepository
            .Setup(repo => repo.GetSetting())
            .ReturnsAsync((Setting)null!);

        // Act
        Func<Task> act = async () => await _settingGetterService.GetSetting();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No settings found");
    }

}
