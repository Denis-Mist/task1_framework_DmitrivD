namespace Pr1.MinWebService.Domain;

/// <summary>
/// Элемент предметной области - товар в каталоге.
/// </summary>
public sealed record Item(Guid Id, string Name, decimal Price);