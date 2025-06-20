using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;

namespace SimpleService.Api.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        
        _logger.LogError(exception, "An unhandled exception occurred");

        var result = exception switch
        {
            ValidationException validationEx => HandleValidationException(validationEx),
            ArgumentException argumentEx => HandleArgumentException(argumentEx),
            InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx),
            _ => HandleGenericException(exception)
        };

        context.Result = result;
        context.ExceptionHandled = true;
    }

    private static IActionResult HandleValidationException(ValidationException exception)
    {
        var errors = exception.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "One or more validation errors occurred.",
            status = 400,
            errors
        });
    }

    private static IActionResult HandleArgumentException(ArgumentException exception)
    {
        return new BadRequestObjectResult(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = exception.Message
        });
    }

    private static IActionResult HandleInvalidOperationException(InvalidOperationException exception)
    {
        return new ConflictObjectResult(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            title = "Conflict",
            status = 409,
            detail = exception.Message
        });
    }

    private static IActionResult HandleGenericException(Exception exception)
    {
        return new ObjectResult(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "An error occurred while processing your request.",
            status = 500
        })
        {
            StatusCode = 500
        };
    }
}