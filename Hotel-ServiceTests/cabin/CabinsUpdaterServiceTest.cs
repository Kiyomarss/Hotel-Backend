using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using Entities;
using FluentAssertions;
using Hotel_Core.DTO;
using Hotel_Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using Services;
using Xunit;

public class IFormFileSpecimen : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(IFormFile))
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            return fileMock.Object;
        }
        return new NoSpecimen();
    }
}

public class CabinsUpdaterServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICabinsRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CabinsUpdaterService _cabinsUpdaterService;


    public CabinsUpdaterServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new IFormFileSpecimen());

        _mockRepository = new Mock<ICabinsRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _cabinsUpdaterService = new CabinsUpdaterService(_mockRepository.Object, _mockUnitOfWork.Object, Mock.Of<ILogger<CabinsGetterService>>());
    }
    
    [Fact]
    public async Task UpdateCabin_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Act
        Func<Task> act = async () => await _cabinsUpdaterService.UpdateCabin(null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateCabin_ShouldUpdateCabin_WhenDataIsValid()
    {
        // Arrange
        var cabinRequest = _fixture.Create<CabinUpsertRequest>();
        var updatedCabin = _fixture.Create<Cabin>();
        var expectedResponse = updatedCabin.ToCabinResponse();

        _mockRepository.Setup(repo => repo.UpdateCabin(cabinRequest)).ReturnsAsync(updatedCabin);
        _mockUnitOfWork.Setup(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<CabinResponse>>>()))
                       .Returns<Func<Task<CabinResponse>>>(operation => operation());

        // Act
        var result = await _cabinsUpdaterService.UpdateCabin(cabinRequest);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        _mockRepository.Verify(repo => repo.UpdateCabin(cabinRequest), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.ExecuteTransactionAsync(It.IsAny<Func<Task<CabinResponse>>>()), Times.Once);
    }
}
