namespace Common.General
{
    public record ApiResponse<T>(bool Success, string Message, T? Data);
}
