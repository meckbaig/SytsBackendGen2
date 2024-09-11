using Microsoft.AspNetCore.Diagnostics;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Web.Structure.CustomProblemDetails;

namespace SytsBackendGen2.Web.Structure;

/// <summary>
/// Global exceptions handler
/// </summary>
public class CustomExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    /// <summary>
    /// Register known exception types and handlers.
    /// </summary>
    public CustomExceptionHandler()
    {
        _exceptionHandlers = new()
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(JsonPatchExceptionWithPosition), HandleJsonPatchException },
            //{ typeof(NotFoundException), HandleNotFoundException },
            //{ typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
        };
    }

    /// <summary>
    /// Handle the exception according to registered types
    /// </summary>
    /// <param name="httpContext">Request context</param>
    /// <param name="ex">Error to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception ex,
        CancellationToken cancellationToken)
    {
        var exceptionType = ex.GetType();

        if (_exceptionHandlers.ContainsKey(exceptionType))
        {
            await _exceptionHandlers[exceptionType].Invoke(httpContext, ex);
            return true;
        }

        if (await CheckForPostgresException(httpContext, ex))
            await HandlePostgresException(httpContext, ex);
        else
            await HandleUnhandledException(httpContext, ex);

        return false;
    }

    private async Task<bool> CheckForPostgresException(HttpContext httpContext, Exception ex)
    {
        bool isPostgresException = false;
        Exception? exception = ex;
        do
        {
            Type type = exception.GetType();
            if (exception is Npgsql.PostgresException || exception is Npgsql.NpgsqlException)
            {
                isPostgresException = true;
                break;
            }
            exception = exception.InnerException;
        } while (exception is not null);
        return isPostgresException;
    }

    private async Task HandleUnhandledException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        ErrorItem error = new(ex.Message, "UnhandledException");
        await httpContext.Response.WriteAsJsonAsync(new
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
            Title = "Unhandled exception occurred.",
            Errors = new Dictionary<string, ErrorItem[]>
            {
                { "Undefined", [error] }
            }
        });
    }

    private async Task HandlePostgresException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        ErrorItem error = new(ex.Message, "DatabaseException");
        await httpContext.Response.WriteAsJsonAsync(new
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
            Title = "Database exception occurred.",
            Errors = new Dictionary<string, ErrorItem[]>
            {
                { "Undefined", [error] }
            }
        });
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        if (ex is ValidationException exception)
        {
            await httpContext.Response.WriteAsJsonAsync(new CustomValidationProblemDetails(exception)
            {
                Status = httpContext.Response.StatusCode,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            });
        }
        //else if (ex as System.ComponentModel.DataAnnotations.ValidationException != null)
        //{
        //    var exception = (System.ComponentModel.DataAnnotations.ValidationException)ex;
        //    await httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
        //    {
        //        Status = httpContext.Response.StatusCode,
        //        Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
        //        Title = "One or more validation errors occurred.",
        //        Detail = exception.Message
        //    });
        //}
    }

    private async Task HandleJsonPatchException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        ErrorItem error = new(ex.Message, "JsonPatchException");
        await httpContext.Response.WriteAsJsonAsync(new
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
            Title = "Exception occurred while executing Json Patch expression.",
            Errors = new Dictionary<string, ErrorItem[]>
            {
                { $"patch.operations[{(ex as JsonPatchExceptionWithPosition).Position}]", [error] }
            }
        });

    }

    private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        if (ex is ForbiddenAccessException exception)
        {
            await httpContext.Response.WriteAsJsonAsync(new CustomAccessProblemDetails(exception)
            {
                Status = StatusCodes.Status403Forbidden,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            });
        }
    }
}