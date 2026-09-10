using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientesAPI.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await ManejarExcepcionAsync(context, exception);
        }
    }

    private async Task ManejarExcepcionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Recurso no encontrado"),

            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "Conflicto"),

            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, "Conflicto de concurrencia"),

            DbUpdateException =>
                (StatusCodes.Status409Conflict, "Conflicto de persistencia"),

            _ =>
                (StatusCodes.Status500InternalServerError, "Error interno")
        };

        if (statusCode >= 500)
            _logger.LogError(exception, "Error no controlado");
        else
            _logger.LogWarning(exception, "Error controlado");

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == 500
                ? "Ocurrió un error interno."
                : exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(
            problem,
            context.RequestAborted);
    }
}