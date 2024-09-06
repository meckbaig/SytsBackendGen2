using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Linq;

namespace SytsBackendGen2.Application.Common.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("Access denied. See errors for details.")
    {
        Errors = new KeyValuePair<string, ErrorItem[]>();
    }

    public ForbiddenAccessException(ValidationFailure failure) : this()
    {
        Errors = new KeyValuePair<string, ErrorItem[]>(
                    failure.PropertyName,
                    [new ErrorItem(failure.ErrorMessage, failure.ErrorCode)]
                );
    }

    public ForbiddenAccessException(KeyValuePair<string, ErrorItem[]> failures) : this()
    {
        Errors = failures;
    }

    public ForbiddenAccessException(string key, ErrorItem[] failures) : this()
    {
        Errors = new KeyValuePair<string, ErrorItem[]>(key, failures);
    }

    public new KeyValuePair<string, ErrorItem[]> Errors { get; }
}

public enum ForbiddenAccessErrorCode
{
    ForbiddenAccessValidator

}