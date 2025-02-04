using Moq;
using FluentAssertions;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Hotel_Core.ServiceContracts;
using RepositoryContracts;
using Services;
using Entities;
using Hotel_Core.DTO;

public class SettingUpdaterServiceTests
{
    private readonly Mock<ISettingRepository> _mockSettingRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<SettingGetterService>> _mockLogger;
    private readonly SettingUpdaterService _settingUpdaterService;
    private readonly Fixture _fixture;

    public SettingUpdaterServiceTests()
    {
        _mockSettingRepository = new Mock<ISettingRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<SettingGetterService>>();
        _settingUpdaterService = new SettingUpdaterService(_mockSettingRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task UpdateSetting_ShouldThrowArgumentNullException_WhenSettingUpdateRequestIsNull()
    {
        // Arrange
        Setting setting = null;

        // Act
        Func<Task> act = async () => await _settingUpdaterService.UpdateSetting(setting);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateSetting_ShouldUpdateSetting_WhenRequestIsValid()
    {
        // Arrange
        var settingUpdateRequest = _fixture.Build<Setting>()
            .With(s => s.MinBookingLength, 3)
            .With(s => s.MaxBookingLength, 10)
            .With(s => s.MaxGuestsPerBooking, 5)
            .With(s => s.BreakfastPrice, 15)
            .Create();

        var setting = _fixture.Build<Setting>()
            .With(s => s.MinBookingLength, 3)
            .With(s => s.MaxBookingLength, 10)
            .With(s => s.MaxGuestsPerBooking, 5)
            .With(s => s.BreakfastPrice, 15)
            .Create();

        _mockSettingRepository
            .Setup(repo => repo.UpdateSetting(It.IsAny<Setting>()))
            .ReturnsAsync(setting);

        _mockUnitOfWork
            .Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<Setting>>>()))
            .ReturnsAsync(setting);

        // Act
        var result = await _settingUpdaterService.UpdateSetting(settingUpdateRequest);

        // Assert
        result.Should().NotBeNull();
        result.MinBookingLength.Should().Be(setting.MinBookingLength);
        result.MaxBookingLength.Should().Be(setting.MaxBookingLength);
        result.MaxGuestsPerBooking.Should().Be(setting.MaxGuestsPerBooking);
        result.BreakfastPrice.Should().Be(setting.BreakfastPrice);
        _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<Setting>>>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSetting_ShouldRollbackTransaction_WhenUpdateFails()
    {
        // Arrange
        var setting = _fixture.Build<Setting>()
            .With(s => s.MinBookingLength, 3)
            .With(s => s.MaxBookingLength, 10)
            .With(s => s.MaxGuestsPerBooking, 5)
            .With(s => s.BreakfastPrice, 15.5)
            .Create();

        _mockSettingRepository
            .Setup(repo => repo.UpdateSetting(It.IsAny<Setting>()))
            .ThrowsAsync(new Exception("Update failed"));

        _mockUnitOfWork
            .Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<Setting>>>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        Func<Task> act = async () => await _settingUpdaterService.UpdateSetting(setting);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Update failed");
        _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<Setting>>>()), Times.Once);
    }
}