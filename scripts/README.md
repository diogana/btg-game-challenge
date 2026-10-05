# Scripts de suporte

- smoke-test.py: fluxo HTTP real; usa BFF_URL ou localhost:5000. Desativa os dados criados após devolver, preservando histórico.
- export-openapi.py: exporta contratos da execução; --check detecta divergência e --offline valida os YAMLs sem APIs.
- package.py: cria ZIP na pasta acima da raiz, removendo builds, dependências, dados, segredos e arquivos transitórios.

Requisitos Python para contratos: `python3 -m pip install -r scripts/requirements.txt`. Smoke e empacotamento usam somente biblioteca padrão.
