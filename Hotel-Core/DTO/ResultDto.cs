namespace Hotel_Core.DTO;

public class ResultDto<T>
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; }
    public T? Data { get; init; }

    private ResultDto(bool isSuccess, string message, T? data = default)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
    }

    public static ResultDto<T> Success(T? data, string message = "Operation successful.") => new(true, message, data);
    public static ResultDto<T> Failure(string message = "Operation failed.") => new(false, message);
}