using MediatR;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.Extensions.Logging;

namespace SytsBackendGen2.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehaviour(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (JsonPatchException ex)
        {
            _logger.LogError("JsonPatchException occured: {Message} {@FailedOperation}",
                ex.Message,
                ex.FailedOperation);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occured: {@Exception}", ex);
            if (ex.InnerException != null)
                throw new Exception(ex.InnerException.Message, ex);
            throw;
        }
    }
}
