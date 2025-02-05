namespace Hotel_Core.DTO;

public record GetBookingsQuery(
    string? Status,
    string? SortBy,
    string? SortDirection,
    int Page = 1,
    int PageSize = 10);

public record BookingsResult(List<BookingResponse> Bookings, int TotalCount);