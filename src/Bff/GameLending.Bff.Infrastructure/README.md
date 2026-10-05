# GameLending.Bff.Infrastructure

Application agrega respostas para as telas; Infrastructure usa HttpClientFactory e propaga o Bearer; API publica contratos de interface.

Este projeto corresponde à camada Infrastructure. Para executar a aplicação, use o projeto GameLending.Bff.Api. Configuração: Services__Core, Services__Auth, Web__Origin e Jwt__*.

Build na raiz: `dotnet build src/Bff/GameLending.Bff.Infrastructure/GameLending.Bff.Infrastructure.csproj`. Testes da área: `dotnet test tests/GameLending.Bff.UnitTests`.

Detalhes, endpoints e execução: [README da área](../README.md). Contratos de negócio e decisões estão na pasta docs da raiz.
