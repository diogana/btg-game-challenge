# Auth — Autenticação demonstrativa

Domain define credenciais; Application orquestra emissão; Infrastructure implementa validator e assinatura; API publica token e validate.

Execução: `dotnet run --project src/Auth/GameLending.Auth.Api --no-launch-profile --urls http://localhost:5002` na raiz, com as variáveis do README principal. O Dockerfile da API suporta Compose.

Configuração: Jwt__SigningKey, Jwt__Issuer, Jwt__Audience, Jwt__ExpirationMinutes e ASPNETCORE_ENVIRONMENT=Challenge.

Endpoints: POST /api/v1/auth/token e GET /api/v1/auth/validate. Contrato completo em `GameLending.Auth.Api/openapi.yaml`; Swagger em `/swagger`.

Testes: `dotnet test tests/GameLending.Auth.UnitTests`; integração HTTP em `tests/GameLending.Auth.IntegrationTests`. Integração Core exige PostgreSQL. Veja [testes](../../docs/testing.md).

Leia [arquitetura](../../docs/architecture.md), [segurança](../../docs/security.md) e [domínio](../../docs/business.md).
