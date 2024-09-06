using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Linq;

namespace SytsBackendGen2.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, ErrorItem[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(
                e => e.PropertyName,
                e => new ErrorItem(e.ErrorMessage, e.ErrorCode))
            .ToDictionary(
                failureGroup => failureGroup.Key, 
                failureGroup => failureGroup.ToArray());
    }

    public ValidationException(ModelStateDictionary failures) : this()
    {
        Errors = failures
            .Where(e => e.Value.ValidationState == ModelValidationState.Invalid)
            .GroupBy(
                e => e.Key,
                e => new ErrorItem(e.Value.Errors[0].ErrorMessage, ValidationErrorCode.DataTypeValidator.ToString()))
            .ToDictionary(
                failureGroup => failureGroup.Key,
                failureGroup => failureGroup.ToArray());

    }

    public ValidationException(IDictionary<string, ErrorItem[]> failures) : this()
    {
        Errors = failures;
    }

    public ValidationException(string key, ErrorItem[] failures) : this()
    {
        Errors = new Dictionary<string, ErrorItem[]> { { key, failures } };
    }

    public new IDictionary<string, ErrorItem[]> Errors { get; }
}

public record ErrorItem
{
    public string message { get; set; }
    public string code { get; set; }

    public ErrorItem(string message, string code)
    {
        this.message = message;
        this.code = code;
    }

    public ErrorItem(string message, ValidationErrorCode code)
    {
        this.message = message;
        this.code = code.ToString();
    }
}

public enum ValidationErrorCode
{
    NotSpecifiedValidationErrorValidator,
    PropertyDoesNotExistValidator,
    ExpressionIsUndefinedValidator,
    PropertyIsNotFilterableValidator,
    CanNotCreateExpressionValidator,
    DataTypeValidator,
    PasswordIncorrectValidator,

    EntityIdValidator,
    RefreshTokenNotValid,
    RefreshTokenExpired
}