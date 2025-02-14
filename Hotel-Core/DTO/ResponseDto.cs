namespace Hotel_Core.DTO;

public record MessageResponse(string Message);

public record DataResponse<T>(T? Data, string Message = "");
