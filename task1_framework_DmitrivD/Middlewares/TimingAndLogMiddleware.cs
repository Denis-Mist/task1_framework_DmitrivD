using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using task1_framework_DmitrivD.Services;

namespace task1_framework_DmitrivD.Middlewares;

/// <summary>
/// Третий middleware в конвейере.
/// Измеряет время выполнения запроса и записывает сводную информацию в журнал.
/// 
/// Порядок: ТРЕТИЙ - измеряет чистое время выполнения бизнес-логики,
/// после того как Request ID создан и обработка ошибок настроена.
/// </summary>
public sealed class TimingAndLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimingAndLogMiddleware> _logger;

    public TimingAndLogMiddleware(RequestDelegate next, ILogger<TimingAndLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestId = RequestId.GetOrCreate(context);
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            // Логируем каждый запрос с метриками производительности
            _logger.LogInformation(
                "Запрос обработан. requestId={RequestId} method={Method} path={Path} status={Status} timeMs={TimeMs}",
                requestId,
                context.Request.Method,
                context.Request.Path.Value ?? string.Empty,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds
            );
        }
    }
}
