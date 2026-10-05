# GameLending Web

Angular 22.2.1 com componentes standalone, formulários reativos, services por funcionalidade e rotas lazy. Node 24.19.0, npm 11.9.0; versões exatas e lock file incluídos.

```bash
npm ci
npm start
npm run build
npm test
npm run e2e
```

`npm start` atende em http://localhost:4200 e encaminha `/api` para BFF em localhost:5000. A imagem Docker serve o build via nginx, encaminhando o mesmo prefixo para bff-api:8080.

Telas: login, dashboard, jogos, detalhes de jogo, amigos, resumo de amigo, empréstimos e histórico. CRUD, pesquisa, paginação, seleção de disponíveis e devolução usam APIs reais. Buscas de seleção limitam a página a 100 resultados; refine o texto para encontrar outros registros.

AuthService guarda JWT em sessionStorage. O interceptor só o envia ao caminho próprio `/api/v1/`; 401 encerra sessão, 403 apresenta falta de permissão. Credenciais não são persistidas.

Vitest testa serviços, interceptor, guard e telas. Cypress executa o fluxo completo com nomes únicos. O browser/binário não faz parte do ZIP. Para apenas compilar sem baixar Cypress: `CYPRESS_INSTALL_BINARY=0 npm ci`; isso não equivale a executar E2E.

Saída de produção: dist/game-lending-web/browser. O build e os 14 testes foram executados; limitações de browser no ambiente de entrega estão em docs/validation-report.md na raiz.
