namespace ContaCorrente.Api.Models.Response;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? ErrorType { get; set; }
    public List<string>? Errors { get; set; }
}