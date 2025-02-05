using Hotel_Core.DTO;
using Hotel_UI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceContracts;

public class BookingsControllerTests
{
    private readonly Mock<IBookingsGetterService> _mockBookingsGetterService;
    private readonly BookingsController _controller;

    public BookingsControllerTests()
    {
        _mockBookingsGetterService = new Mock<IBookingsGetterService>();
        _controller = new BookingsController(null, _mockBookingsGetterService.Object, null);
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
}