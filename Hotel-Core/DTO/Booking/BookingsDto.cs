namespace Hotel_Core.DTO;

public record GetBookingsQuery(string? Status,string? SortBy,string? SortDirection,int Page = 1,int PageSize = 10);

public record BookingsResult(List<BookingsItemResult> Bookings, int TotalCount);

public record BookingsItemResult(string Status, int TotalPrice, string CabinName, string CountryFlag, string CreateAt);

public record DeleteBookingResult(bool IsDeleted);

public record GetStaysTodayActivityBookingResult(string Status, int TotalPrice, int NumGuests, string CountryFlag);

public record GetStaysAfterDateResult(string Status, string CreateAt);

public record BookingResult(string Status, int TotalPrice, string CabinName,  string CountryFlag, string Nationality);

public record AddExternalLoginRequest(string UserId, string LoginProvider, string ProviderKey, string ProviderDisplayName);

public record RemoveExternalLoginRequest(string UserId, string LoginProvider, string ProviderKey);

public record ExternalLoginRequest(string LoginProvider, string ProviderKey, string ProviderDisplayName, String? EmailFromProvider);