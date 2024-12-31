using AutoFixture;
using Entities;
using FluentAssertions;
using Hotel_Core.Domain.Entities;
using Hotel_Core.DTO;
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
        private readonly Mock<ILogger<BookingsUpdaterService>> _mockBookingsUpdaterServiceLogger;
        private readonly BookingsUpdaterService _bookingsUpdaterService;
        private readonly Fixture _fixture;

        public BookingsGetterServiceTests()
        {
            _mockRepository = new Mock<IBookingsRepository>();
            _mockBookingsGetterServiceLogger = new Mock<ILogger<BookingsGetterService>>();
            _bookingsGetterService = new BookingsGetterService(_mockRepository.Object, _mockBookingsGetterServiceLogger.Object);
            _mockBookingsUpdaterServiceLogger = new Mock<ILogger<BookingsUpdaterService>>();
            _bookingsUpdaterService = new BookingsUpdaterService(_mockRepository.Object, _mockBookingsUpdaterServiceLogger.Object);
            _fixture = new Fixture();
        }

        #region BookingsGetterService
        
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
        public async Task GetStaysTodayActivity_ShouldReturnTodaysBookings()
        {
            // Arrange
            var bookings = _fixture.CreateMany<Booking>(1).ToList();

            _mockRepository.Setup(repo => repo.GetStaysTodayActivity())
                .ReturnsAsync(bookings);

            // Act
            var result = await _bookingsGetterService.GetStaysTodayActivity();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(bookings.First().Id, result.First().Id);
        }

        [Fact]
        public async Task GetBookingByBookingId_ShouldReturnBookingResponse_WhenBookingExists()
        {
            // Arrange
            var booking = _fixture.Build<Booking>()
                .With(b => b.Id, Guid.NewGuid())
                .With(b => b.StartDate, DateTime.UtcNow.AddDays(-3))
                .With(b => b.EndDate, DateTime.UtcNow)
                .With(b => b.NumNights, 3)
                .With(b => b.NumGuests, 2)
                .With(b => b.CabinPrice, 200)
                .With(b => b.ExtrasPrice, 50)
                .With(b => b.TotalPrice, 250)
                .With(b => b.Status, "checked-in")
                .With(b => b.HasBreakfast, true)
                .With(b => b.IsPaid, true)
                .With(b => b.Observations, "No observations")
                .With(b => b.CabinId, Guid.NewGuid())
                .With(b => b.GuestId, Guid.NewGuid())
                .With(b => b.CreateAt, DateTime.UtcNow.AddDays(-5))
                .With(b => b.Guest, _fixture.Build<Guest>()
                    .With(g => g.FullName, "John Doe")
                    .With(g => g.Nationality, "USA")
                    .With(g => g.CountryFlag, "🇺🇸")
                    .With(g => g.Email, "john.doe@example.com")
                    .With(g => g.NationalID, "123456789")
                    .Create())
                .With(b => b.Cabin, _fixture.Build<Cabin>()
                    .With(c => c.Name, "Luxury Cabin")
                    .Create())
                .Create();

            _mockRepository.Setup(repo => repo.GetBookingByBookingId(booking.Id))
                .ReturnsAsync(booking);

            // Act
            var result = await _bookingsGetterService.GetBookingByBookingId(booking.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(booking.ToBookingResponse(), options => options.ExcludingMissingMembers());
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
        }

        #endregion
    }
}
