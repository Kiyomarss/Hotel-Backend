using AutoFixture;
using Hotel_Core.DTO;
using Hotel_UI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceContracts;

public class BookingsControllerTests
{
    private readonly Mock<IBookingsGetterService> _mockBookingsGetterService;
    private readonly Mock<IBookingsDeleterService> _mockBookingsDeleterService;
    private readonly BookingsController _controller;
    private readonly Fixture _fixture;

    public BookingsControllerTests()
    {
        _mockBookingsGetterService = new Mock<IBookingsGetterService>();
        _mockBookingsDeleterService = new Mock<IBookingsDeleterService>();
        _controller = new BookingsController(_mockBookingsDeleterService.Object, _mockBookingsGetterService.Object, null);
        _fixture = new Fixture();

    }

    [Fact]
    public async Task GetBookings_ReturnsOkResult_WithBookingsAndTotalCount()
    {
        // Arrange
        var query = new GetBookingsQuery(null, null, null, 1, 10);
        var bookings = new List<BookingResponse>
        {
            new BookingResponse
            {
                Id = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1)
            }
        };

        var paginatedResult = new PaginatedResult<BookingResponse>
        {
            Items = bookings, TotalCount = 1
        };

        _mockBookingsGetterService.Setup(service => service.GetBookings(query.Status, query.SortBy, query.SortDirection, query.Page, query.PageSize)).ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetBookings(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<BookingsResult>(okResult.Value);

        Assert.Equal(bookings, returnValue.Bookings);
        Assert.Equal(1, returnValue.TotalCount);
    }
    
    [Fact]
    public async Task GetBookings_ReturnsEmptyList_WhenNoBookingsExist()
    {
        // Arrange
        var query = new GetBookingsQuery(null, null, null, 1, 10);
        var paginatedResult = new PaginatedResult<BookingResponse>
        {
            Items = new List<BookingResponse>(),
            TotalCount = 0
        };

        _mockBookingsGetterService.Setup(service => service.GetBookings(query.Status, query.SortBy, query.SortDirection, query.Page, query.PageSize))
                                  .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetBookings(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<BookingsResult>(okResult.Value);
        Assert.Empty(returnValue.Bookings);
        Assert.Equal(0, returnValue.TotalCount);
    }
    
    [Fact]
    public async Task Delete_ReturnsOkResult_WithIsDeletedTrue()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        _mockBookingsDeleterService.Setup(service => service.DeleteBooking(bookingId))
                                   .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(bookingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<DeleteBookingResult>(okResult.Value);
        Assert.True(returnValue.IsDeleted);
    }

    [Fact]
    public async Task Delete_ReturnsOkResult_WithIsDeletedFalse()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        _mockBookingsDeleterService.Setup(service => service.DeleteBooking(bookingId))
                                   .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(bookingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<DeleteBookingResult>(okResult.Value);
        Assert.False(returnValue.IsDeleted);
    }
    
    [Fact]
    public async Task GetStaysTodayActivity_ReturnsOkResult_WithExpectedData()
    {
        // Arrange
        var staysTodayActivity = _fixture.CreateMany<GetStaysTodayActivityBookingResult>(3).ToList();

        _mockBookingsGetterService
            .Setup(s => s.GetStaysTodayActivity())
            .ReturnsAsync(staysTodayActivity);

        // Act
        var result = await _controller.GetStaysTodayActivity();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<List<GetStaysTodayActivityBookingResult>>(okResult.Value);

        Assert.NotNull(response);
        Assert.Equal(staysTodayActivity.Count, response.Count);
        Assert.Equal(staysTodayActivity, response);
    }

}