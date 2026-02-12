namespace Pr1.MinWebService.Domain;

/// <summary>
/// Единый формат ошибки для всех клиентов API.
/// Содержит код ошибки, сообщение и идентификатор запроса для трассировки.
/// </summary>
public sealed record ErrorResponse(string Code, string Message, string RequestId);
