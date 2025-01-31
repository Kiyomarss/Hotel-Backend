using AutoFixture;
using Entities;
using FluentAssertions;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
using Hotel_Core.ServiceContracts;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using Services;

namespace Hotel_ServiceTests
{
    public class CabinsGetterServiceTests
    {
        private readonly Mock<ICabinsRepository> _mockRepository;
        private readonly Mock<ILogger<CabinsGetterService>> _mockCabinsGetterServiceLogger;
        private readonly CabinsGetterService _cabinsGetterService;
        private readonly Fixture _fixture;

        public CabinsGetterServiceTests()
        {
            _mockRepository = new Mock<ICabinsRepository>();
            _mockCabinsGetterServiceLogger = new Mock<ILogger<CabinsGetterService>>();
            _cabinsGetterService = new CabinsGetterService(_mockRepository.Object, _mockCabinsGetterServiceLogger.Object);
            _fixture = new Fixture();
        }
        
        
        [Fact]
        public async Task GetCabins_ShouldReturnListOfCabinResponses_WhenCabinsExist()
        {
            // Arrange
            var cabins = _fixture.CreateMany<Cabin>(3).ToList();
            var expectedResponses = cabins.Select(c => c.ToCabinResponse()).ToList();

            _mockRepository.Setup(repo => repo.GetCabins()).ReturnsAsync(cabins);

            // Act
            var result = await _cabinsGetterService.GetCabins();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<CabinResponse>>(); 
            result.Should().HaveCount(cabins.Count);
            result.Should().BeEquivalentTo(expectedResponses);

            _mockRepository.Verify(repo => repo.GetCabins(), Times.Once);
        }

        [Fact]
        public async Task GetCabins_ShouldReturnEmptyList_WhenNoCabinsExist()
        {
            // Arrange
            var cabins = new List<Cabin>();
            _mockRepository.Setup(repo => repo.GetCabins()).ReturnsAsync(cabins);

            // Act
            var result = await _cabinsGetterService.GetCabins();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<CabinResponse>>();

            _mockRepository.Verify(repo => repo.GetCabins(), Times.Once);
        }

        [Fact]
        public async Task FindCabinById_ShouldReturnCabin_WhenCabinExists()
        {
            // Arrange
            var cabinId = Guid.NewGuid();
            var cabin = _fixture.Build<Cabin>()
                .With(b => b.Id, cabinId)
                .Create();

            _mockRepository.Setup(repo => repo.FindCabinById(cabinId))
                .ReturnsAsync(cabin);

            // Act
            var result = await _cabinsGetterService.GetCabinByCabinId(cabinId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(cabin, options => options.ExcludingMissingMembers());

            _mockRepository.Verify(repo => repo.FindCabinById(cabinId), Times.Once);
        }

        [Fact]
        public async Task FindCabinById_ShouldReturnNull_WhenCabinDoesNotExist()
        {
            // Arrange
            var cabinId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.FindCabinById(cabinId))
                .ReturnsAsync((Cabin?)null);

            // Act
            var result = await _cabinsGetterService.GetCabinByCabinId(cabinId);

            // Assert
            result.Should().BeNull();

            _mockRepository.Verify(repo => repo.FindCabinById(cabinId), Times.Once);
        }
    }
}