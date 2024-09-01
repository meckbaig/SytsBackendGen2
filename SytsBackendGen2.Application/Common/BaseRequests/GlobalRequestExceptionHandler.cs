using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SytsBackendGen2.Application.Common.BaseRequests
{
    public class GlobalRequestExceptionHandler<TRequest, TResponse, TException>
  : IRequestExceptionHandler<TRequest, TResponse, TException>
      where TResponse : BaseResponse, new()
      where TException : Exception
    {
        private static readonly Type[] _notLoggedErrorTypes =
        [
            typeof(Exceptions.ValidationException)
        ];

        private readonly ILogger<GlobalRequestExceptionHandler<TRequest, TResponse, TException>> _logger;
        public GlobalRequestExceptionHandler(
           ILogger<GlobalRequestExceptionHandler<TRequest, TResponse, TException>> logger)
        {
            _logger = logger;
        }
        public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state,
            CancellationToken cancellationToken)
        {
            var ex = exception.Demystify();
            if (!_notLoggedErrorTypes.Contains(exception.GetType()))
                _logger.LogError(ex, "Something went wrong while handling request of type {@requestType}", typeof(TRequest));
            var response = new TResponse { Message = ex.Message };
            response.SetException(ex);
            state.SetHandled(response);
            return Task.CompletedTask;
        }
    }
}
