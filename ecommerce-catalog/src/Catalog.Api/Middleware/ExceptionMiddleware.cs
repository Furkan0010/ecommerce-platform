using System.Text.Json;
using Catalog.Application.Common;

namespace Catalog.Api.Middleware;

/// <summary>
/// BlogPlatform'daki gibi merkezi hata yakalama: beklenmeyen istisnalar
/// 500 + Result.Failure formatında döner.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen hata oluştu.");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var result = Result.Failure("Beklenmeyen bir hata oluştu.");
            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}
