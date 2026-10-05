using GameLending.Bff.Application;
using GameLending.Bff.Infrastructure;
using GameLending.Security;
using GameLending.ApiSupport;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDocumentation("GameLending BFF"); builder.Services.AddGameLendingSecurity(builder.Configuration);
builder.Services.AddHttpContextAccessor(); builder.Services.AddTransient<BearerPropagationHandler>();
builder.Services.AddHttpClient<IBackendClient, BackendClient>(client =>
{ client.BaseAddress = new Uri(builder.Configuration["Services:Core"] ?? "http://localhost:5001/api/v1/"); client.Timeout = TimeSpan.FromSeconds(15); })
.AddHttpMessageHandler<BearerPropagationHandler>();
builder.Services.AddHttpClient("auth", client =>
{ client.BaseAddress = new Uri(builder.Configuration["Services:Auth"] ?? "http://localhost:5002/api/v1/"); client.Timeout = TimeSpan.FromSeconds(15); })
.AddHttpMessageHandler<BearerPropagationHandler>();
builder.Services.AddScoped<ScreenQueries>(); builder.Services.AddHealthChecks();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins(builder.Configuration["Web:Origin"] ?? "http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()!.Error;
    var status = error is DownstreamException downstream ? downstream.Status : 500;
    app.Logger.LogWarning("BFF request failed {Status} {TraceId}", status, context.TraceIdentifier);
    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = status,
        Title = "Request failed",
        Detail = error is DownstreamException ? error.Message : null,
        Extensions = { ["traceId"] = context.TraceIdentifier }
    });
}));
app.UseDocumentation(); app.UseCors(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
app.MapHealthChecks("/health/live").AllowAnonymous(); app.MapHealthChecks("/health/ready").AllowAnonymous();
app.Run();
public partial class Program;
