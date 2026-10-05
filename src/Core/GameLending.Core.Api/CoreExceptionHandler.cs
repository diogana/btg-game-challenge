using GameLending.Core.Application;
using GameLending.Core.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace GameLending.Core.Api;

public sealed class CoreExceptionHandler(ILogger<CoreExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception error, CancellationToken ct)
    {
        var status = error switch
        {
            NotFoundException => 404,
            DomainException => 409,
            ArgumentException => 400,
            DbUpdateException { InnerException: PostgresException { SqlState: "23505" } } => 409,
            _ => 500
        };
        if (status == 500) logger.LogError("Request failed {TraceId} {ErrorType}", context.TraceIdentifier, error.GetType().Name);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = status == 500 ? "Internal server error" : status == 409 ? "Business conflict" : "Request failed",
            Detail = status == 500 ? null : error is DbUpdateException ? "Game already has an active loan." : error.Message,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        }, ct);
        return true;
    }
}
