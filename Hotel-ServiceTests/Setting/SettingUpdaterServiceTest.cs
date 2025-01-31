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
        SettingUpsertRequest settingUpdateRequest = null;

        // Act
        Func<Task> act = async () => await _settingUpdaterService.UpdateSetting(settingUpdateRequest);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateSetting_ShouldUpdateSetting_WhenRequestIsValid()
    {
        // Arrange
        var settingUpdateRequest = _fixture.Build<SettingUpsertRequest>()
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

        var settingResponse = new SettingResponse
        {
            MinBookingLength = 3,
            MaxBookingLength = 10,
            MaxGuestsPerBooking = 5,
            BreakfastPrice = 15
        };

        _mockSettingRepository
            .Setup(repo => repo.UpdateSetting(It.IsAny<Setting>()))
            .ReturnsAsync(setting);

        _mockUnitOfWork
            .Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<SettingResponse>>>()))
            .ReturnsAsync(settingResponse);

        // Act
        var result = await _settingUpdaterService.UpdateSetting(settingUpdateRequest);

        // Assert
        result.Should().NotBeNull();
        result.MinBookingLength.Should().Be(settingResponse.MinBookingLength);
        result.MaxBookingLength.Should().Be(settingResponse.MaxBookingLength);
        result.MaxGuestsPerBooking.Should().Be(settingResponse.MaxGuestsPerBooking);
        result.BreakfastPrice.Should().Be(settingResponse.BreakfastPrice);
        _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<SettingResponse>>>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSetting_ShouldRollbackTransaction_WhenUpdateFails()
    {
        // Arrange
        var settingUpdateRequest = _fixture.Build<SettingUpsertRequest>()
            .With(s => s.MinBookingLength, 3)
            .With(s => s.MaxBookingLength, 10)
            .With(s => s.MaxGuestsPerBooking, 5)
            .With(s => s.BreakfastPrice, 15.5)
            .Create();

        _mockSettingRepository
            .Setup(repo => repo.UpdateSetting(It.IsAny<Setting>()))
            .ThrowsAsync(new Exception("Update failed"));

        _mockUnitOfWork
            .Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<SettingResponse>>>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        Func<Task> act = async () => await _settingUpdaterService.UpdateSetting(settingUpdateRequest);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Update failed");
        _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<SettingResponse>>>()), Times.Once);
    }
}