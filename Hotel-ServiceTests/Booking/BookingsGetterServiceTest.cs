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
        public async Task GetBookingsAfterDate_ShouldReturnFilteredBookings()
        {
            // Arrange
            var date = DateTime.UtcNow.AddDays(-1);

            var fixture = new Fixture();

            var bookings = fixture.Build<Booking>()
                .With(b => b.CreateAt, DateTime.UtcNow.AddDays(-2))
                .With(b => b.StartDate, DateTime.UtcNow.AddDays(-3))
                .With(b => b.EndDate, DateTime.UtcNow.AddDays(-1))
                .CreateMany(3)
                .ToList();

            var expectedResponses = bookings.Select(b => b.ToBookingResponse()).ToList();

            _mockRepository.Setup(repo => repo.GetBookingsAfterDate(date))
                .ReturnsAsync(bookings);

            // Act
            var result = await _bookingsGetterService.GetBookingsAfterDate(date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponses.Count, result.Count);

            for (int i = 0; i < expectedResponses.Count; i++)
            {
                Assert.Equal(expectedResponses[i].Id, result[i].Id);
                Assert.Equal(expectedResponses[i].Guests.FullName, result[i].Guests.FullName);
                Assert.Equal(expectedResponses[i].Cabins.Name, result[i].Cabins.Name);
            }
            
            _mockRepository.Verify(repo => repo.GetBookingsAfterDate(date), Times.Once);
        }
        
        [Fact]
        public async Task GetStaysAfterDate_ShouldReturnFilteredStays()
        {
            // Arrange
            var date = DateTime.UtcNow.AddDays(-1);

            var fixture = new Fixture();

            var validBookings = fixture.Build<Booking>()
                .With(b => b.Status, "checked-in")
                .With(b => b.CreateAt, DateTime.UtcNow.AddDays(-2))
                .CreateMany(3)
                .ToList();

            var unconfirmedBookings = fixture.Build<Booking>()
                .With(b => b.Status, "unconfirmed")
                .With(b => b.CreateAt, DateTime.UtcNow.AddDays(-3))
                .CreateMany(2)
                .ToList();

            var allBookings = validBookings
                .Concat(unconfirmedBookings)
                .ToList();

            _mockRepository.Setup(repo => repo.GetStaysAfterDate(date))
                .ReturnsAsync(allBookings);

            // Act
            var result = await _bookingsGetterService.GetStaysAfterDate(date);

            // Assert
            result.Should().NotBeNull();

            result.Count.Should().Be(validBookings.Count + unconfirmedBookings.Count);

            foreach (var booking in validBookings.Concat(unconfirmedBookings))
            {
                var correspondingResult = result.FirstOrDefault(r => r.Id == booking.Id);

                correspondingResult.Should().NotBeNull();

                correspondingResult.Status.Should().Be(booking.Status);
            }
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
        public async Task GetBookingByBookingId_ShouldReturnBookingResponse_WhenBookingExists()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var fixture = new Fixture();

            var booking = fixture.Build<Booking>()
                                 .With(b => b.Id, bookingId)
                                 .Create();

            var expectedResponse = booking.ToBookingResponse();

            _mockRepository.Setup(repo => repo.GetBookingByBookingId(bookingId))
                           .ReturnsAsync(booking);

            // Act
            var result = await _bookingsGetterService.GetBookingByBookingId(bookingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.Guests.FullName, result.Guests.FullName);
            Assert.Equal(expectedResponse.Cabins.Name, result.Cabins.Name);

            _mockRepository.Verify(repo => repo.GetBookingByBookingId(bookingId), Times.Once);
        }

        [Fact]
        public async Task GetBookingByBookingId_ShouldThrowArgumentException_WhenBookingDoesNotExist()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.GetBookingByBookingId(bookingId))
                           .ReturnsAsync((Booking?)null);

            // Act
            Func<Task> act = async () => await _bookingsGetterService.GetBookingByBookingId(bookingId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("Given Booking id doesn't exist");

            _mockRepository.Verify(repo => repo.GetBookingByBookingId(bookingId), Times.Once);
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