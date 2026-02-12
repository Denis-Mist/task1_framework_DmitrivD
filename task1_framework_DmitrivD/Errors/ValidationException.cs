namespace Pr1.MinWebService.Errors;

/// <summary>
/// Исключение для случаев нарушения правил валидации входных данных.
/// Возвращает HTTP 400.
/// </summary>
public sealed class ValidationException : DomainException
{
    public ValidationException(string message)
        : base(code: "validation", message: message, statusCode: 400)
    {
    }
}
