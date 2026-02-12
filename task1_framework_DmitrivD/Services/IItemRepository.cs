using Pr1.MinWebService.Domain;

namespace task1_framework_DmitrivD.Services;

/// <summary>
/// Интерфейс репозитория для работы с элементами каталога.
/// Абстрагирует хранилище данных от бизнес-логики.
/// </summary>
public interface IItemRepository
{
    /// <summary>
    /// Получает все элементы из хранилища.
    /// </summary>
    IReadOnlyCollection<Item> GetAll();

    /// <summary>
    /// Получает элемент по идентификатору.
    /// </summary>
    /// <returns>Элемент или null, если не найден.</returns>
    Item? GetById(Guid id);

    /// <summary>
    /// Создаёт новый элемент в хранилище.
    /// </summary>
    Item Create(string name, decimal price);
}
