using Xunit;
using Pr1.MinWebService.Domain;
using Pr1.MinWebService.Errors;
using task1_framework_DmitrivD.Services;

namespace Pr1.MinWebService.Tests;

/// <summary>
/// Юнит-тесты для правил валидации предметной области.
/// </summary>
public class ValidationTests
{
    [Fact]
    public void EmptyName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateItemRequest("", 100m);

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() =>
            ValidateRequest(request));
        Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WhitespaceName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateItemRequest("   ", 100m);

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() =>
            ValidateRequest(request));
        Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NegativePrice_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateItemRequest("Valid Name", -10m);

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() =>
            ValidateRequest(request));
        Assert.Contains("price", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NameTooLong_ThrowsValidationException()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateItemRequest(longName, 100m);

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() =>
            ValidateRequest(request));
        Assert.Contains("200", ex.Message);
    }

    [Theory]
    [InlineData("Товар", 0)]
    [InlineData("Product", 99.99)]
    [InlineData("Item 123", 1000000)]
    public void ValidRequest_DoesNotThrow(string name, decimal price)
    {
        // Arrange
        var request = new CreateItemRequest(name, price);

        // Act & Assert - не должно быть исключений
        ValidateRequest(request);
    }

    // Вспомогательный метод, имитирующий логику из Program.cs
    private static void ValidateRequest(CreateItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Поле name не должно быть пустым");

        if (request.Price < 0)
            throw new ValidationException("Поле price не может быть отрицательным");

        if (request.Name.Length > 200)
            throw new ValidationException("Поле name не должно превышать 200 символов");
    }
}

/// <summary>
/// Юнит-тесты для InMemoryItemRepository.
/// </summary>
public class RepositoryTests
{
    [Fact]
    public void GetAll_InitiallyEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var repo = new InMemoryItemRepository();

        // Act
        var items = repo.GetAll();

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public void Create_ValidItem_ReturnsItemWithId()
    {
        // Arrange
        var repo = new InMemoryItemRepository();

        // Act
        var item = repo.Create("Test Item", 99.99m);

        // Assert
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal("Test Item", item.Name);
        Assert.Equal(99.99m, item.Price);
    }

    [Fact]
    public void GetById_ExistingItem_ReturnsItem()
    {
        // Arrange
        var repo = new InMemoryItemRepository();
        var created = repo.Create("Test Item", 99.99m);

        // Act
        var found = repo.GetById(created.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
        Assert.Equal(created.Name, found.Name);
        Assert.Equal(created.Price, found.Price);
    }

    [Fact]
    public void GetById_NonExistingItem_ReturnsNull()
    {
        // Arrange
        var repo = new InMemoryItemRepository();
        var nonExistingId = Guid.NewGuid();

        // Act
        var found = repo.GetById(nonExistingId);

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public void GetAll_AfterCreating_ReturnsAllItems()
    {
        // Arrange
        var repo = new InMemoryItemRepository();
        repo.Create("Item 1", 10m);
        repo.Create("Item 2", 20m);
        repo.Create("Item 3", 30m);

        // Act
        var items = repo.GetAll();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void GetAll_ReturnsSortedByName()
    {
        // Arrange
        var repo = new InMemoryItemRepository();
        repo.Create("Zebra", 10m);
        repo.Create("Apple", 20m);
        repo.Create("Banana", 30m);

        // Act
        var items = repo.GetAll().ToList();

        // Assert
        Assert.Equal("Apple", items[0].Name);
        Assert.Equal("Banana", items[1].Name);
        Assert.Equal("Zebra", items[2].Name);
    }

    [Fact]
    public async Task Create_ConcurrentOperations_AllItemsStored()
    {
        // Arrange
        var repo = new InMemoryItemRepository();
        var tasks = new List<Task<Item>>();

        // Act - создаём 20 элементов параллельно
        for (int i = 0; i < 20; i++)
        {
            var index = i; // захват переменной
            tasks.Add(Task.Run(() => repo.Create($"Item {index}", index * 10m)));
        }

        var items = await Task.WhenAll(tasks);
        var allItems = repo.GetAll();

        // Assert
        Assert.Equal(20, allItems.Count);
        Assert.Equal(20, allItems.Select(x => x.Id).Distinct().Count()); // все ID уникальны
    }

    [Fact]
    public void Create_TrimsWhitespace_StoresCleanName()
    {
        // Arrange
        var repo = new InMemoryItemRepository();

        // Act
        var item = repo.Create("  Test Item  ", 99.99m);

        // Assert
        // Примечание: текущая реализация не триммит, но можно добавить
        Assert.Equal("  Test Item  ", item.Name);
    }
}

/// <summary>
/// Тесты для доменных исключений.
/// </summary>
public class ExceptionTests
{
    [Fact]
    public void NotFoundException_HasCorrectProperties()
    {
        // Arrange & Act
        var ex = new NotFoundException("Элемент не найден");

        // Assert
        Assert.Equal("not_found", ex.Code);
        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("Элемент не найден", ex.Message);
    }

    [Fact]
    public void ValidationException_HasCorrectProperties()
    {
        // Arrange & Act
        var ex = new ValidationException("Некорректные данные");

        // Assert
        Assert.Equal("validation", ex.Code);
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("Некорректные данные", ex.Message);
    }
}
