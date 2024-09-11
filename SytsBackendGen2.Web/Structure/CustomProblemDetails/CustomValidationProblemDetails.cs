using System.Text.Json.Serialization;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Extensions.StringExtensions;

namespace SytsBackendGen2.Web.Structure.CustomProblemDetails
{
    /// <summary>
    /// Represents a custom implementation of <see cref="ValidationProblemDetails"/> for handling validation problems.
    /// </summary>
    public class CustomValidationProblemDetails : ValidationProblemDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomValidationProblemDetails"/> class.
        /// </summary>
        public CustomValidationProblemDetails() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomValidationProblemDetails"/> class with the specified <see cref="ValidationException"/>.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> to initialize the instance with.</param>
        public CustomValidationProblemDetails(ValidationException exception)
        {
            Title = exception.Message;

            // Convert the errors dictionary from the exception to a dictionary with keys in camel case
            Errors = exception.Errors.ToDictionary(
                kvp => string.Join(".", kvp.Key.Split('.').Select(x => x.ToCamelCase())),
                kvp => kvp.Value);
        }

        /// <summary>
        /// Gets or sets the errors dictionary for the problem.
        /// </summary>
        /// <remarks>
        /// This property is overridden to provide a new implementation that returns a dictionary with keys in camel case.
        /// </remarks>
        [JsonPropertyName("errors")]
        public new IDictionary<string, ErrorItem[]> Errors { get; }
    }
}
