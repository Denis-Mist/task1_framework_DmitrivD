using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace task1_framework_DmitrivD.Services;

/// <summary>
/// Утилита для генерации и хранения уникального идентификатора запроса.
/// Request ID используется для сквозной трассировки запроса через все компоненты системы.
/// </summary>
public static class RequestId
{
    private const string ItemKey = "request_id";
    private const string HeaderName = "X-Request-Id";

    // Разрешённые символы для Request ID (буквы, цифры, дефис)
    private static readonly Regex Allowed = new("^[a-zA-Z0-9\\-]{1,64}$", RegexOptions.Compiled);

    /// <summary>
    /// Получает существующий или создаёт новый Request ID.
    /// Приоритет: 1) HttpContext.Items, 2) заголовок X-Request-Id, 3) новый GUID.
    /// </summary>
    public static string GetOrCreate(HttpContext context)
    {
        // Проверяем кэш в HttpContext.Items
        if (context.Items.TryGetValue(ItemKey, out var existing) && existing is string s && s.Length > 0)
            return s;

        // Пытаемся получить из заголовка запроса
        var candidate = context.Request.Headers[HeaderName].FirstOrDefault();
        var requestId = !string.IsNullOrWhiteSpace(candidate) && Allowed.IsMatch(candidate!)
            ? candidate!
            : Guid.NewGuid().ToString("N"); // Генерируем новый

        // Сохраняем в Items и добавляем в заголовок ответа
        context.Items[ItemKey] = requestId;
        context.Response.Headers[HeaderName] = requestId;

        return requestId;
    }
}
