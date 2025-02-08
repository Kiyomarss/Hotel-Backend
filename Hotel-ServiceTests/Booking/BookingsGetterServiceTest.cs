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
    public class BookingsGetterServiceTests
    {
        private readonly Mock<IBookingsRepository> _mockRepository;
        private readonly Mock<ILogger<BookingsGetterService>> _mockBookingsGetterServiceLogger;
        private readonly BookingsGetterService _bookingsGetterService;
        private readonly Fixture _fixture;

        public BookingsGetterServiceTests()
        {
            _mockRepository = new Mock<IBookingsRepository>();
            _mockBookingsGetterServiceLogger = new Mock<ILogger<BookingsGetterService>>();
            _bookingsGetterService = new BookingsGetterService(_mockRepository.Object, _mockBookingsGetterServiceLogger.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetBookings_ShouldReturnPaginatedResult()
        {
            // Arrange
            var bookings = _fixture.CreateMany<Booking>(2).ToList();
            var paginatedResult = new PaginatedResult<Booking>
            {
                Items = bookings,
                TotalCount = bookings.Count
            };

            _mockRepository.Setup(repo => repo.GetBookings(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(paginatedResult);

            // Act
            var result = await _bookingsGetterService.GetBookings("confirmed", "startDate", "asc", 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookings.Count, result.TotalCount);
            Assert.Equal(bookings.Count, result.Items.Count);
            _mockRepository.Verify(repo => repo.GetBookings("confirmed", "startDate", "asc", 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetStaysAfterDate_ReturnsListOfGetStaysAfterDateResult()
        {
            // Arrange
            var date = DateTime.UtcNow.AddDays(-5);

            var bookings = _fixture.Build<Booking>()
                                   .With(b => b.Status, "confirmed")
                                   .With(b => b.CreateAt, date.AddDays(1))
                                   .CreateMany(1)
                                   .Concat(_fixture.Build<Booking>()
                                                   .With(b => b.Status, "checked-in")
                                                   .With(b => b.CreateAt, date.AddDays(2))
                                                   .CreateMany(1))
                                   .ToList();

            _mockRepository.Setup(repo => repo.GetStaysAfterDate(date))
                           .ReturnsAsync(bookings);

            // Act
            var result = await _bookingsGetterService.GetStaysAfterDate(date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookings.Count, result.Count);

            for (int i = 0; i < bookings.Count; i++)
            {
                Assert.Equal(bookings[i].Status, result[i].Status);
                Assert.Equal(bookings[i].CreateAt.ToString(), result[i].CreateAt);
            }
        }

        [Fact]
        public async Task GetStaysAfterDate_ReturnsEmptyList_WhenNoBookingsExist()
        {
            // Arrange
            var date = DateTime.UtcNow;
            _mockRepository.Setup(repo => repo.GetStaysAfterDate(date))
                           .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _bookingsGetterService.GetStaysAfterDate(date);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        
        [Fact]
        public async Task GetStaysTodayActivity_ReturnsListOfGetStaysTodayActivityBookingResult()
        {
            // Arrange
            var statuses = new[] { "checked-out", "checked-in" };
            var countryFlags = new[] { "🇺🇸", "🇬🇧" };
            var totalPrices = new[] { 100, 150 };
            var numGuests = new[] { 2, 3 };

            var bookings = statuses
                           .Select((status, index) => _fixture.Build<Booking>()
                                                              .With(b => b.Status, status)
                                                              .With(b => b.TotalPrice, totalPrices[index])
                                                              .With(b => b.NumGuests, numGuests[index])
                                                              .With(b => b.Guest, new Guest { CountryFlag = countryFlags[index] })
                                                              .Create())
                           .ToList();

            _mockRepository.Setup(repo => repo.GetStaysTodayActivity())
                           .ReturnsAsync(bookings);

            // Act
            var result = await _bookingsGetterService.GetStaysTodayActivity();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookings.Count, result.Count);

            result.Zip(bookings, (actual, expected) =>
            {
                Assert.Equal(expected.Status, actual.Status);
                Assert.Equal(expected.TotalPrice, actual.TotalPrice);
                Assert.Equal(expected.NumGuests, actual.NumGuests);
                Assert.Equal(expected.Guest.CountryFlag, actual.CountryFlag);
                return true;
            }).ToList();
        }

        [Fact]
        public async Task GetStaysTodayActivity_ReturnsEmptyList_WhenNoBookingsExist()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetStaysTodayActivity())
                .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _bookingsGetterService.GetStaysTodayActivity();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBookingByBookingId_ReturnsBooking_WhenBookingExists()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = _fixture.Build<Booking>()
                                  .With(b => b.Id, bookingId)
                                  .With(b => b.Status, "Confirmed")
                                  .With(b => b.TotalPrice, 200)
                                  .With(b => b.Cabin, new Cabin { Name = "Deluxe Suite" })
                                  .With(b => b.Guest, new Guest { CountryFlag = "🇫🇷", Nationality = "French" })
                                  .Create();

            _mockRepository.Setup(repo => repo.GetBookingByBookingId(bookingId))
                           .ReturnsAsync(booking);

            // Act
            var result = await _bookingsGetterService.GetBookingByBookingId(bookingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(booking.Status, result.Status);
            Assert.Equal(booking.TotalPrice, result.TotalPrice);
            Assert.Equal(booking.Cabin.Name, result.CabinName);
            Assert.Equal(booking.Guest.CountryFlag, result.CountryFlag);
            Assert.Equal(booking.Guest.Nationality, result.Nationality);
        }

        [Fact]
        public async Task GetBookingByBookingId_ThrowsException_WhenBookingDoesNotExist()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.GetBookingByBookingId(bookingId))
                           .ReturnsAsync((Booking?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookingsGetterService.GetBookingByBookingId(bookingId));

            Assert.Equal("Given Booking id doesn't exist", exception.Message);
        }
        
        [Fact]
        public async Task FindBookingById_ShouldReturnBooking_WhenBookingExists()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = _fixture.Build<Booking>()
                .With(b => b.Id, bookingId)
                .Create();

            _mockRepository.Setup(repo => repo.FindBookingById(bookingId))
                .ReturnsAsync(booking);

            // Act
            var result = await _bookingsGetterService.FindBookingById(bookingId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(booking, options => options.ExcludingMissingMembers());

            _mockRepository.Verify(repo => repo.FindBookingById(bookingId), Times.Once);
        }

        [Fact]
        public async Task FindBookingById_ShouldReturnNull_WhenBookingDoesNotExist()
        {
            // Arrange
            var bookingId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.FindBookingById(bookingId))
                .ReturnsAsync((Booking?)null);

            // Act
            var result = await _bookingsGetterService.FindBookingById(bookingId);

            // Assert
            result.Should().BeNull();

            _mockRepository.Verify(repo => repo.FindBookingById(bookingId), Times.Once);
        }
    }
}