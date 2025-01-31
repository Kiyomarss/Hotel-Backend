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
using Entities;
using Hotel_Core.ServiceContracts;
using Xunit;

namespace Hotel_ServiceTests
{
    public class CabinsDeleterServiceTests
    {
        private readonly Mock<ICabinsRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<CabinsGetterService>> _mockLogger;
        private readonly CabinsDeleterService _cabinsDeleterService;
        private readonly Fixture _fixture;

        public CabinsDeleterServiceTests()
        {
            _mockRepository = new Mock<ICabinsRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<CabinsGetterService>>();
            _cabinsDeleterService = new CabinsDeleterService(_mockRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task DeleteCabin_ShouldReturnTrue_WhenCabinExistsAndDeletedSuccessfully()
        {
            // Arrange
            var cabinId = Guid.NewGuid();
            var cabin = _fixture.Build<Cabin>().With(b => b.Id, cabinId).Create();

            _mockRepository.Setup(repo => repo.FindCabinById(cabinId)).ReturnsAsync(cabin);
            _mockUnitOfWork.Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()))
                           .Returns<Func<Task<bool>>>(op => op());
            _mockRepository.Setup(repo => repo.DeleteCabin(cabinId)).ReturnsAsync(true);

            // Act
            var result = await _cabinsDeleterService.DeleteCabin(cabinId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(repo => repo.FindCabinById(cabinId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteCabin(cabinId), Times.Once);
        }

        [Fact]
        public async Task DeleteCabin_ShouldThrowKeyNotFoundException_WhenCabinDoesNotExist()
        {
            // Arrange
            var cabinId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.FindCabinById(cabinId)).ReturnsAsync((Cabin?)null);

            // Act
            Func<Task> act = async () => await _cabinsDeleterService.DeleteCabin(cabinId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Cabin with ID {cabinId} does not exist.");
            _mockRepository.Verify(repo => repo.FindCabinById(cabinId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()), Times.Never);
            _mockRepository.Verify(repo => repo.DeleteCabin(cabinId), Times.Never);
        }

        [Fact]
        public async Task DeleteCabin_ShouldThrowException_WhenDeletionFails()
        {
            // Arrange
            var cabinId = Guid.NewGuid();
            var cabin = _fixture.Build<Cabin>().With(b => b.Id, cabinId).Create();

            _mockRepository.Setup(repo => repo.FindCabinById(cabinId)).ReturnsAsync(cabin);
            _mockUnitOfWork.Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()))
                           .ThrowsAsync(new Exception("Deletion failed"));

            // Act
            Func<Task> act = async () => await _cabinsDeleterService.DeleteCabin(cabinId);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Deletion failed");
            _mockRepository.Verify(repo => repo.FindCabinById(cabinId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<bool>>>()), Times.Once);
        }
    }
}