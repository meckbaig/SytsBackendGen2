namespace SytsBackendGen2.Application.Common.BaseRequests
{
    public class ResponseException : BaseResponse
    {
        public new string Message { get; set; }
        public IDictionary<string, string[]> Errors { get; }

        public ResponseException(string message, IDictionary<string, string[]> failures)
        {
            Message = message;
            Errors = failures;
        }

        public ResponseException(string message)
        {
            Message = message;
        }
    }
}
