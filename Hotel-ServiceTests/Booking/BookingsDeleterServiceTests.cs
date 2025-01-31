using AutoFixture;
using FluentAssertions;
using Hotel_Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Core.ServiceContracts;
using Xunit;

namespace Hotel_ServiceTests
{
    public class BookingsDeleterServiceTests
    {
        private readonly Mock<IBookingsRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<BookingsGetterService>> _mockLogger;
        private readonly BookingsDeleterService _bookingsDeleterService;
        private readonly Fixture _fixture;

        public BookingsDeleterServiceTests()
        {
            _mockRepository = new Mock<IBookingsRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<BookingsGetterService>>();
            _bookingsDeleterService = new BookingsDeleterService(_mockRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task DeleteBooking_ShouldReturnTrue_WhenBookingExistsAndDeletedSuccessfully()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = _fixture.Build<Booking>().With(b => b.Id, bookingId).Create();

            _mockRepository.Setup(repo => repo.FindBookingById(bookingId)).ReturnsAsync(booking);
            _mockUnitOfWork.Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()))
                           .Returns<Func<Task<bool>>>(op => op());
            _mockRepository.Setup(repo => repo.DeleteBooking(bookingId)).ReturnsAsync(true);

            // Act
            var result = await _bookingsDeleterService.DeleteBooking(bookingId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(repo => repo.FindBookingById(bookingId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteBooking(bookingId), Times.Once);
        }

        [Fact]
        public async Task DeleteBooking_ShouldThrowKeyNotFoundException_WhenBookingDoesNotExist()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.FindBookingById(bookingId)).ReturnsAsync((Booking?)null);

            // Act
            Func<Task> act = async () => await _bookingsDeleterService.DeleteBooking(bookingId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Booking with ID {bookingId} does not exist.");
            _mockRepository.Verify(repo => repo.FindBookingById(bookingId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()), Times.Never);
            _mockRepository.Verify(repo => repo.DeleteBooking(bookingId), Times.Never);
        }

        [Fact]
        public async Task DeleteBooking_ShouldThrowException_WhenDeletionFails()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = _fixture.Build<Booking>().With(b => b.Id, bookingId).Create();

            _mockRepository.Setup(repo => repo.FindBookingById(bookingId)).ReturnsAsync(booking);
            _mockUnitOfWork.Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()))
                           .ThrowsAsync(new Exception("Deletion failed"));

            // Act
            Func<Task> act = async () => await _bookingsDeleterService.DeleteBooking(bookingId);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Deletion failed");
            _mockRepository.Verify(repo => repo.FindBookingById(bookingId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()), Times.Once);
        }
    }
}
