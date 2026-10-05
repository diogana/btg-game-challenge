# Core — Domínio e persistência

Domain contém invariantes. Application separa comandos e consultas. Infrastructure implementa PostgreSQL, migrations e batch. API contém controllers finos.

Execução: `dotnet run --project src/Core/GameLending.Core.Api --no-launch-profile --urls http://localhost:5001` na raiz, com as variáveis do README principal. O Dockerfile da API suporta Compose.

Configuração: ConnectionStrings__Lending, Jwt__*, CatalogImport__Path, CatalogImport__BatchSize e CatalogImport__Enabled.

Endpoints: CRUD /api/v1/friends e /api/v1/games; /api/v1/loans e /return; /api/v1/statistics. Contrato completo em `GameLending.Core.Api/openapi.yaml`; Swagger em `/swagger`.

Testes: `dotnet test tests/GameLending.Core.UnitTests`; integração HTTP em `tests/GameLending.Core.IntegrationTests`. Integração Core exige PostgreSQL. Veja [testes](../../docs/testing.md).

Leia [arquitetura](../../docs/architecture.md), [segurança](../../docs/security.md) e [domínio](../../docs/business.md).
