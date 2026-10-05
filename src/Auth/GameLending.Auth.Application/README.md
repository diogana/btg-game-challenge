# GameLending.Auth.Application

Domain define credenciais; Application orquestra emissão; Infrastructure implementa validator e assinatura; API publica token e validate.

Este projeto corresponde à camada Application. Para executar a aplicação, use o projeto GameLending.Auth.Api. Configuração: Jwt__SigningKey, Jwt__Issuer, Jwt__Audience, Jwt__ExpirationMinutes e ASPNETCORE_ENVIRONMENT=Challenge.

Build na raiz: `dotnet build src/Auth/GameLending.Auth.Application/GameLending.Auth.Application.csproj`. Testes da área: `dotnet test tests/GameLending.Auth.UnitTests`.

Detalhes, endpoints e execução: [README da área](../README.md). Contratos de negócio e decisões estão na pasta docs da raiz.
