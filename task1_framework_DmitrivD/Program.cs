using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using Pr1.MinWebService.Domain;
using Pr1.MinWebService.Errors;
using Microsoft.OpenApi;
using task1_framework_DmitrivD;
using task1_framework_DmitrivD.Services;
using task1_framework_DmitrivD.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Настройка сериализации для компактных ответов
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddSingleton<IItemRepository, InMemoryItemRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MinWebService API", Version = "v1" });
});

var app = builder.Build();

// Конвейер обработки запросов (порядок критически важен)
app.UseMiddleware<RequestIdMiddleware>();       // 1. Генерация Request ID
app.UseMiddleware<ErrorHandlingMiddleware>();   // 2. Перехват исключений
app.UseMiddleware<TimingAndLogMiddleware>();    // 3. Логирование и замер времени

// REST API endpoints
app.MapGet("/api/items", (IItemRepository repo) =>
{
    return Results.Ok(repo.GetAll());
});

app.MapGet("/api/items/{id:guid}", (Guid id, IItemRepository repo) =>
{
    var item = repo.GetById(id);
    if (item is null)
        throw new NotFoundException($"Элемент с ID {id} не найден");

    return Results.Ok(item);
});

app.MapPost("/api/items", (HttpContext ctx, CreateItemRequest request, IItemRepository repo) =>
{
    // Валидация входных данных
    if (string.IsNullOrWhiteSpace(request.Name))
        throw new ValidationException("Поле name не должно быть пустым");

    if (request.Price < 0)
        throw new ValidationException("Поле price не может быть отрицательным");

    if (request.Name.Length > 200)
        throw new ValidationException("Поле name не должно превышать 200 символов");

    var created = repo.Create(request.Name.Trim(), request.Price);

    var location = $"/api/items/{created.Id}";
    ctx.Response.Headers.Location = location;

    return Results.Created(location, created);
});

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MinWebService API v1"));

app.Run();

// Требуется для интеграционных тестов
public partial class Program { }