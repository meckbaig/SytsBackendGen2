namespace SytsBackendGen2.Application.Common.Exceptions;

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

    public ErrorItem(string message, ForbiddenAccessErrorCode code)
    {
        this.message = message;
        this.code = code.ToString();
    }
}
