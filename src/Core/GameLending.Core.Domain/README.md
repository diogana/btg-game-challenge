# GameLending.Core.Domain

Domain contém invariantes. Application separa comandos e consultas. Infrastructure implementa PostgreSQL, migrations e batch. API contém controllers finos.

Este projeto corresponde à camada Domain. Para executar a aplicação, use o projeto GameLending.Core.Api. Configuração: ConnectionStrings__Lending, Jwt__*, CatalogImport__Path, CatalogImport__BatchSize e CatalogImport__Enabled.

Build na raiz: `dotnet build src/Core/GameLending.Core.Domain/GameLending.Core.Domain.csproj`. Testes da área: `dotnet test tests/GameLending.Core.UnitTests`.

Detalhes, endpoints e execução: [README da área](../README.md). Contratos de negócio e decisões estão na pasta docs da raiz.
