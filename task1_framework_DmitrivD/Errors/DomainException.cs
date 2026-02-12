namespace Pr1.MinWebService.Errors;

/// <summary>
/// Базовое исключение предметной области.
/// Инкапсулирует код ошибки и HTTP статус код для единообразной обработки.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string code, string message, int statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Машиночитаемый код ошибки (например, "not_found", "validation").
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// HTTP статус код для ответа клиенту.
    /// </summary>
    public int StatusCode { get; }
}
