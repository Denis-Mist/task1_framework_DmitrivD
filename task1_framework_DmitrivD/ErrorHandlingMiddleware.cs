using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pr1.MinWebService.Domain;
using Pr1.MinWebService.Errors;
using task1_framework_DmitrivD.Services;

namespace task1_framework_DmitrivD;

/// <summary>
/// Второй middleware в конвейере.
/// Единая обработка всех исключений и формирование согласованных ответов об ошибках.
/// 
/// Порядок: ВТОРОЙ - оборачивает все последующие компоненты try-catch блоком,
/// преобразует любые исключения в единообразный формат ErrorResponse.
/// </summary>
public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestId = RequestId.GetOrCreate(context);

        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            // Ожидаемые ошибки предметной области (валидация, not found)
            _logger.LogWarning(ex, 
                "Ошибка предметной области. requestId={RequestId} code={ErrorCode}", 
                requestId, ex.Code);
            await WriteError(context, ex.StatusCode, ex.Code, ex.Message, requestId);
        }
        catch (Exception ex)
        {
            // Непредвиденные ошибки - требуют расследования
            _logger.LogError(ex, 
                "Непредвиденная ошибка. requestId={RequestId}", 
                requestId);
            await WriteError(context, 500, "internal_error", "Внутренняя ошибка сервера", requestId);
        }
    }

    private static async Task WriteError(
        HttpContext context, 
        int statusCode, 
        string code, 
        string message, 
        string requestId)
    {
        // Защита от повторной записи в ответ
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new ErrorResponse(code, message, requestId);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
