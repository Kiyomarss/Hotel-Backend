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

    #region GetBookings

    [Fact]
    public async Task GetBookings_ReturnsOkResult_WithBookingsAndTotalCount()
    {
        // Arrange
        var query = new GetBookingsQuery(null, null, null, 1, 10);
        var bookings = _fixture.CreateMany<BookingsItemResult>(2).ToList();

        var paginatedResult = new PaginatedResult<BookingsItemResult>
        {
            Items = bookings, TotalCount = 2
        };

        _mockBookingsGetterService.Setup(service => service.GetBookings(query.Status, query.SortBy, query.SortDirection, query.Page, query.PageSize)).ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetBookings(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<BookingsResult>(okResult.Value);

        Assert.Equal(bookings, returnValue.Bookings);
        Assert.Equal(2, returnValue.TotalCount);
    }
    
    [Fact]
    public async Task GetBookings_ReturnsEmptyList_WhenNoBookingsExist()
    {
        // Arrange
        var query = new GetBookingsQuery(null, null, null, 1, 10);
        var paginatedResult = new PaginatedResult<BookingsItemResult>
        {
            Items = new List<BookingsItemResult>(),
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

    #endregion

    #region Delete

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

    #endregion

    #region GetStaysTodayActivity

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

    #endregion

    #region GetBookingByBookingId

    [Fact]
    public async Task GetBooking_ReturnsOk_WhenBookingExists()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var bookingResponse = _fixture.Create<BookingResult>();

        _mockBookingsGetterService.Setup(s => s.GetBookingByBookingId(bookingId))
                                  .ReturnsAsync(bookingResponse);

        // Act
        var result = await _controller.GetBooking(bookingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<BookingResult>(okResult.Value);

        Assert.NotNull(response);
        Assert.Equal(bookingResponse.Status, response.Status);
        Assert.Equal(bookingResponse.TotalPrice, response.TotalPrice);
        Assert.Equal(bookingResponse.CabinName, response.CabinName);
        Assert.Equal(bookingResponse.CountryFlag, response.CountryFlag);
        Assert.Equal(bookingResponse.Nationality, response.Nationality);
    }

    [Fact]
    public async Task GetBooking_ReturnsNotFound_WhenBookingDoesNotExist()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        _mockBookingsGetterService.Setup(s => s.GetBookingByBookingId(bookingId))
                                  .ReturnsAsync((BookingResult?)null);

        // Act
        var result = await _controller.GetBooking(bookingId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<MessageResponse>(notFoundResult.Value);

        Assert.Equal("Booking not found", response.Message);
    }

    #endregion

    #region GetStaysAfterDate

    [Fact]
    public async Task GetStaysAfterDate_ReturnsOkResult_WithBookingData()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-5);

        var bookings = _fixture.Build<GetStaysAfterDateResult>()
                               .With(b => b.Status, "confirmed")
                               .With(b => b.CreateAt, date.AddDays(1).ToString())
                               .CreateMany(2)
                               .ToList();

        _mockBookingsGetterService
            .Setup(s => s.GetStaysAfterDate(date))
            .ReturnsAsync(bookings);

        // Act
        var result = await _controller.GetStaysAfterDate(date);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<List<GetStaysAfterDateResult>>(okResult.Value);

        Assert.NotNull(response);
        Assert.Equal(bookings.Count, response.Count);

        for (int i = 0; i < bookings.Count; i++)
        {
            Assert.Equal(bookings[i].Status, response[i].Status);
            Assert.Equal(bookings[i].CreateAt, response[i].CreateAt);
        }
    }

    [Fact]
    public async Task GetStaysAfterDate_ReturnsEmptyList_WhenNoBookingsExist()
    {
        // Arrange
        var date = DateTime.UtcNow;
        _mockBookingsGetterService
            .Setup(s => s.GetStaysAfterDate(date))
            .ReturnsAsync(new List<GetStaysAfterDateResult>());

        // Act
        var result = await _controller.GetStaysAfterDate(date);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<List<GetStaysAfterDateResult>>(okResult.Value);

        Assert.NotNull(response);
        Assert.Empty(response);
    }

    #endregion

}