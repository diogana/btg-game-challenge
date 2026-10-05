using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc;
namespace GameLending.ApiSupport;

public static class ApiDocumentation
{
    public static void AddDocumentation(this IServiceCollection services, string title)
    {
        services.AddControllers(); services.AddProblemDetails(); services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1", Description = "Gerenciamento de empréstimos de jogos." });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
            options.OperationFilter<SecurityOperationFilter>();
            options.DocumentFilter<HealthDocumentationFilter>();
        });
    }
    public static void UseDocumentation(this WebApplication app)
    {
        app.UseSwagger(options => { options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1; });
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
    }
}
public sealed class SecurityOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        var attributes = context.MethodInfo.GetCustomAttributes(true).Concat(context.MethodInfo.DeclaringType!.GetCustomAttributes(true));
        if (attributes.OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any()) operation.Security = [];
        var policies = attributes.OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Where(x => x.Policy != null).Select(x => x.Policy).ToArray();
        if (policies.Length > 0) operation.Description = "Permissões exigidas (scope e role): " + string.Join(", ", policies);
        foreach (var code in new[] { "400", "401", "403", "404", "409", "500" })
            operation.Responses!.TryAdd(code, new OpenApiResponse
            {
                Description = code switch { "400" => "Entrada inválida", "401" => "Token ausente ou inválido", "403" => "Permissão insuficiente", "404" => "Recurso não encontrado", "409" => "Conflito de negócio", _ => "Erro interno" },
                Content = new Dictionary<string, OpenApiMediaType> { ["application/problem+json"] = new() { Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository) } }
            });
    }
}

public sealed class HealthDocumentationFilter : Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter
{
    public void Apply(OpenApiDocument document, Swashbuckle.AspNetCore.SwaggerGen.DocumentFilterContext context)
    {
        foreach (var route in new[] { "/health/live", "/health/ready" })
        {
            document.Paths.Add(route, new OpenApiPathItem
            {
                Operations = new Dictionary<HttpMethod, OpenApiOperation>
                {
                    [HttpMethod.Get] = new()
                    {
                        Summary = route.EndsWith("live") ? "Verifica se o processo está ativo" : "Verifica prontidão do serviço",
                        Security = [],
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse { Description = "Healthy", Content = new Dictionary<string, OpenApiMediaType> { ["text/plain"] = new() { Schema = new OpenApiSchema { Type = JsonSchemaType.String } } } },
                            ["503"] = new OpenApiResponse { Description = "Unhealthy" }
                        }
                    }
                }
            });
        }
    }
}
