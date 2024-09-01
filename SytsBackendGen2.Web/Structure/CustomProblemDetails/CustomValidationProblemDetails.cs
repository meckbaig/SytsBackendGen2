using FluentValidation.Results;
using SytsBackendGen2.Application.Common.Exceptions;
using System.Text.Json.Serialization;
using SytsBackendGen2.Application.Common.Extensions.StringExtensions;

namespace SytsBackendGen2.Web.Structure.CustomProblemDetails
{
    /// <summary>
    /// Custom implementaticn of <see cref="T:Microsoft.AspNetCore.Mvc.ValidationProblemDetails" />.
    /// </summary>
    public class CustomValidationProblemDetails : ValidationProblemDetails
    {
        public CustomValidationProblemDetails() { }
    
        /// <summary>
        /// Initializes a new instance of <see cref="CustomValidationProblemDetails" />.
        /// </summary>
        /// <param name="errors">Validation errors</param>
        public CustomValidationProblemDetails(IDictionary<string, ErrorItem[]> errors)
        {
            Errors = errors.ToDictionary(
                kvp => string.Join(".", kvp.Key.Split('.').Select(x => x.ToCamelCase())), 
                kvp => kvp.Value);
        }

        [JsonPropertyName("errors")]
        public new IDictionary<string, ErrorItem[]> Errors { get; }
    }
}
