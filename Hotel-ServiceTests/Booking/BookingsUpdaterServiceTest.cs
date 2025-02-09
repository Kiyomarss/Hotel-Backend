using Moq;
using FluentAssertions;
using AutoFixture;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Hotel_Core.Domain.Entities;
using Hotel_Core.ServiceContracts;
using RepositoryContracts;
using Services;
using Hotel_Core.DTO;

public class BookingsUpdaterServiceTests
{
    private readonly Mock<IBookingsRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<BookingsUpdaterService>> _mockLogger;
    private readonly BookingsUpdaterService _bookingsUpdaterService;
    private readonly Fixture _fixture;

    public BookingsUpdaterServiceTests()
    {
        _mockRepository = new Mock<IBookingsRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<BookingsUpdaterService>>();
        _bookingsUpdaterService = new BookingsUpdaterService(_mockRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task UpdateBooking_ShouldThrowArgumentNullException_WhenPatchDocIsNull()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _bookingsUpdaterService.UpdateBooking(bookingId, null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateBooking_ShouldThrowArgumentException_WhenBookingNotFound()
    {
        // Arrange
        var bookingId = _fixture.Create<Guid>();
        var patchDoc = new JsonPatchDocument<Booking>();

        _mockRepository.Setup(repo => repo.GetBookingByBookingId(bookingId)).ReturnsAsync((Booking)null);

        // Act
        Func<Task> act = async () => await _bookingsUpdaterService.UpdateBooking(bookingId, patchDoc);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Booking ID does not exist");
    }

    [Fact]
    public async Task UpdateBooking_ShouldThrowInvalidOperationException_WhenBookingDataIsInvalid()
    {
        // Arrange
        var bookingId = _fixture.Create<Guid>();
        var booking = _fixture.Build<Booking>()
            .With(b => b.StartDate, DateTime.UtcNow.AddDays(2))
            .With(b => b.EndDate, DateTime.UtcNow.AddDays(1)) // تاریخ پایان نامعتبر
            .With(b => b.NumGuests, 1)
            .With(b => b.NumNights, 1)
            .Create();

        var patchDoc = new JsonPatchDocument<Booking>();

        _mockRepository.Setup(repo => repo.GetBookingByBookingId(bookingId)).ReturnsAsync(booking);

        // Act
        Func<Task> act = async () => await _bookingsUpdaterService.UpdateBooking(bookingId, patchDoc);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid booking data");
    }

    [Fact]
    public async Task UpdateBooking_ShouldUpdateBooking_WhenDataIsValid()
    {
        // Arrange
        var bookingId = _fixture.Create<Guid>();
        var booking = _fixture.Build<Booking>()
            .With(b => b.Id, bookingId)
            .With(b => b.StartDate, DateTime.UtcNow)
            .With(b => b.EndDate, DateTime.UtcNow.AddDays(1))
            .With(b => b.NumGuests, 2)
            .With(b => b.NumNights, 1)
            .Create();

        var patchDoc = new JsonPatchDocument<Booking>();
        patchDoc.Replace(b => b.NumGuests, 3);

        _mockRepository.Setup(repo => repo.GetBookingByBookingId(bookingId)).ReturnsAsync(booking);
        _mockRepository.Setup(repo => repo.UpdateBooking(booking)).ReturnsAsync(booking);
        _mockUnitOfWork.Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<BookingResult>>>()))
                       .Returns<Func<Task<BookingResult>>>(operation => operation());

        // Act
        var result = await _bookingsUpdaterService.UpdateBooking(bookingId, patchDoc);

        // Assert
        result.Should().NotBeNull();
        _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<BookingResult>>>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateBooking_ShouldRollbackTransaction_WhenUpdateFails()
    {
        // Arrange
        var bookingId = _fixture.Create<Guid>();
        var booking = _fixture.Build<Booking>()
                              .With(b => b.Id, bookingId)
                              .With(b => b.StartDate, DateTime.UtcNow)
                              .With(b => b.EndDate, DateTime.UtcNow.AddDays(1))
                              .With(b => b.NumGuests, 2)
                              .With(b => b.NumNights, 1)
                              .Create();

        var patchDoc = new JsonPatchDocument<Booking>();

        _mockRepository.Setup(repo => repo.GetBookingByBookingId(bookingId)).ReturnsAsync(booking);
        _mockRepository.Setup(repo => repo.UpdateBooking(booking)).ThrowsAsync(new Exception("Update failed"));
        _mockUnitOfWork.Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<BookingResult>>>()))
                       .ThrowsAsync(new Exception("Update failed"));

        // Act
        Func<Task> act = async () => await _bookingsUpdaterService.UpdateBooking(bookingId, patchDoc);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Update failed");
        _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<BookingResult>>>()), Times.Once);
    }
}
