using Microsoft.AspNetCore.Http;
using task1_framework_DmitrivD.Services;

namespace task1_framework_DmitrivD.Middlewares;

/// <summary>
/// Первый middleware в конвейере.
/// Обеспечивает наличие уникального идентификатора запроса для трассировки.
/// 
/// Порядок: ПЕРВЫЙ - генерирует Request ID, который используется всеми последующими middleware.
/// </summary>
public sealed class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next) => _next = next;

    public Task Invoke(HttpContext context)
    {
        // Получаем или создаём Request ID
        _ = RequestId.GetOrCreate(context);
        return _next(context);
    }
}
