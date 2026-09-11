using CampLink.Application.Exceptions;

namespace CampLink.WebApi.Middleware;

/// <summary>Преобразует доменные исключения в корректные HTTP-ответы.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string message;

        switch (exception)
        {
            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                message = exception.Message;
                break;
            case ConflictException:
                statusCode = StatusCodes.Status409Conflict;
                message = exception.Message;
                break;
            case ValidationException:
                statusCode = StatusCodes.Status400BadRequest;
                message = exception.Message;
                break;
            default:
                _logger.LogError(exception, "Необработанное исключение: {Message}", exception.Message);
                statusCode = StatusCodes.Status500InternalServerError;
                message = "Произошла внутренняя ошибка сервера.";
                break;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { status = statusCode, message });
    }
}
