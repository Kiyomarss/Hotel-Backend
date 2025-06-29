namespace Hotel_Core.DTO;

public record GetBookingsQuery(string? Status,string? SortBy,string? SortDirection,int Page = 1,int PageSize = 10);

public record BookingsResult(List<BookingsItemResult> Bookings, int TotalCount);

public record BookingsItemResult(Guid Id, string Status, int TotalPrice, int NumNights, int NumGuests, string CabinName, string GuestName, string Email, string CountryFlag, string StartDate, string EndDate, string CreateAt);

public record DeleteBookingResult(bool IsDeleted);

public record GetStaysTodayActivityBookingResult(string Status, int TotalPrice, int NumGuests, string  CountryFlag, string FullName);

public record GetBookingsAfterDateResult(int TotalPrice, int ExtrasPrice, string CreateAt);

public record GetStaysAfterDateResult(int NumNights, string Status, string CreateAt);

public record BookingResult(Guid Id, string Status, int TotalPrice, string CabinName,  string CountryFlag, string Nationality);

public record RemoveExternalLoginRequest(string UserId, string LoginProvider, string ProviderKey);

public record ExternalLoginRequest(string LoginProvider, string ProviderKey, string ProviderDisplayName, String? EmailFromProvider);