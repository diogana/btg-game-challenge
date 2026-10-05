# Bff — Backend para a interface

Application agrega respostas para as telas; Infrastructure usa HttpClientFactory e propaga o Bearer; API publica contratos de interface.

Execução: `dotnet run --project src/Bff/GameLending.Bff.Api --no-launch-profile --urls http://localhost:5000` na raiz, com as variáveis do README principal. O Dockerfile da API suporta Compose.

Configuração: Services__Core, Services__Auth, Web__Origin e Jwt__*.

Endpoints: /api/v1/dashboard, /library, /friends/{id}/summary e operações CRUD/loans; autenticação encaminhada ao Auth. Contrato completo em `GameLending.Bff.Api/openapi.yaml`; Swagger em `/swagger`.

Testes: `dotnet test tests/GameLending.Bff.UnitTests`; integração HTTP em `tests/GameLending.Bff.IntegrationTests`. Integração Core exige PostgreSQL. Veja [testes](../../docs/testing.md).

Leia [arquitetura](../../docs/architecture.md), [segurança](../../docs/security.md) e [domínio](../../docs/business.md).
