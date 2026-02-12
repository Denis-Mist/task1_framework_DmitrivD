using System.Collections.Concurrent;
using Pr1.MinWebService.Domain;

namespace task1_framework_DmitrivD.Services;

/// <summary>
/// Простое хранилище элементов в памяти процесса.
/// Использует ConcurrentDictionary для потокобезопасности при параллельных запросах.
/// 
/// ВАЖНО: Данные хранятся только в памяти и будут потеряны при перезапуске приложения.
/// Для production-систем следует использовать персистентное хранилище (БД, Redis и т.д.).
/// </summary>
public sealed class InMemoryItemRepository : IItemRepository
{
    private readonly ConcurrentDictionary<Guid, Item> _items = new();

    /// <summary>
    /// Возвращает все элементы, отсортированные по имени (без учёта регистра).
    /// </summary>
    public IReadOnlyCollection<Item> GetAll()
        => _items.Values
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    /// <summary>
    /// Ищет элемент по идентификатору.
    /// </summary>
    public Item? GetById(Guid id)
        => _items.TryGetValue(id, out var item) ? item : null;

    /// <summary>
    /// Создаёт новый элемент с уникальным идентификатором.
    /// Потокобезопасная операция благодаря ConcurrentDictionary.
    /// </summary>
    public Item Create(string name, decimal price)
    {
        var id = Guid.NewGuid();
        var item = new Item(id, name, price);

        _items[id] = item;
        return item;
    }
}
