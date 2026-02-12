namespace Pr1.MinWebService.Errors;

/// <summary>
/// Исключение для случаев, когда запрошенный ресурс не найден.
/// Возвращает HTTP 404.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(code: "not_found", message: message, statusCode: 404)
    {
    }
}
