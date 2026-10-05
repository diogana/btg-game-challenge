# GameLending.Security

Centraliza JwtSettings, as seis permissões, mapeamento de roles e configuração de JWT Bearer. Auth, Core e BFF compartilham a mesma definição de autorização.

A configuração é resolvida por DI após a construção do host, permitindo substituição em testes. Chaves curtas são rejeitadas. Tokens são verificados localmente; não há chamada remota por requisição.

Testes em Auth.UnitTests, Auth.IntegrationTests e Core.IntegrationTests. Veja docs/security.md para matriz e limitações.
