using System.Net;
using System.Text.Json;
using FluentValidation;
using ContaCorrente.Domain.Exceptions;
using ContaCorrente.Api.Models.Response;

namespace ContaCorrente.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case DomainException domainException:
                response.StatusCode = domainException.ErrorType switch
                {
                    Domain.Enums.ErrorType.INVALID_DOCUMENT => (int)HttpStatusCode.BadRequest,
                    Domain.Enums.ErrorType.USER_UNAUTHORIZED => (int)HttpStatusCode.Unauthorized,
                    Domain.Enums.ErrorType.INVALID_ACCOUNT => (int)HttpStatusCode.BadRequest,
                    Domain.Enums.ErrorType.INACTIVE_ACCOUNT => (int)HttpStatusCode.BadRequest,
                    Domain.Enums.ErrorType.INVALID_VALUE => (int)HttpStatusCode.BadRequest,
                    Domain.Enums.ErrorType.INVALID_TYPE => (int)HttpStatusCode.BadRequest,
                    Domain.Enums.ErrorType.INSUFFICIENT_BALANCE => (int)HttpStatusCode.BadRequest,
                    Domain.Enums.ErrorType.INVALID_TOKEN => (int)HttpStatusCode.Forbidden,
                    Domain.Enums.ErrorType.TOKEN_EXPIRED => (int)HttpStatusCode.Forbidden,
                    _ => (int)HttpStatusCode.BadRequest
                };
                errorResponse.Message = domainException.Message;
                errorResponse.ErrorType = domainException.ErrorType.ToString();
                break;

            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Erro de validação";
                errorResponse.Errors = validationException.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "Não autorizado";
                errorResponse.ErrorType = "USER_UNAUTHORIZED";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "Ocorreu um erro interno no servidor";
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
}

