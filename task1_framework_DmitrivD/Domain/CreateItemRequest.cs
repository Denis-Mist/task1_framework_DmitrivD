namespace Pr1.MinWebService.Domain;

/// <summary>
/// Запрос на создание нового элемента.
/// </summary>
public sealed record CreateItemRequest(string Name, decimal Price);
