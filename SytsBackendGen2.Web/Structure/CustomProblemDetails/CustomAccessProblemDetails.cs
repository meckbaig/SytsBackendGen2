using System.Text.Json.Serialization;
using SytsBackendGen2.Application.Common.Exceptions;

namespace SytsBackendGen2.Web.Structure.CustomProblemDetails
{
    /// <summary>
    /// Represents a custom implementation of <see cref="ValidationProblemDetails"/> for handling access problems.
    /// </summary>
    public class CustomAccessProblemDetails : ValidationProblemDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAccessProblemDetails"/> class.
        /// </summary>
        public CustomAccessProblemDetails() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAccessProblemDetails"/> class with the specified <see cref="ForbiddenAccessException"/>.
        /// </summary>
        /// <param name="exception">The <see cref="ForbiddenAccessException"/> to initialize the instance with.</param>
        public CustomAccessProblemDetails(ForbiddenAccessException exception)
        {
            Title = exception.Message;
            Errors = new Dictionary<string, ErrorItem[]>() { { exception.Errors.Key, exception.Errors.Value } };
        }

        /// <summary>
        /// Gets or sets the errors dictionary for the problem.
        /// </summary>
        /// <remarks>
        /// This property is overridden to provide a new type for the errors dictionary.
        /// </remarks>
        [JsonPropertyName("errors")]
        public new IDictionary<string, ErrorItem[]> Errors { get; }
    }
}
