# Evonautinhas | School Enrollment API

API REST de controle de matriculas escolares em .NET Framework 4.8, ASP.NET Web API 2, Dapper e SQL Server.

## O que existe

- CRUD de alunos com busca e paginacao
- Listagem de turmas com cache em memoria
- Matricula com validacao de aluno, vagas, duplicidade e transacao
- Relatorio SQL de alunos por turma
- UI estatica em `UI/` com busca, paginacao, cadastro e ocupacao das turmas
- Testes unitarios com NUnit e Moq

## Pre-requisitos

- Windows
- Visual Studio 2022 ou .NET SDK com suporte a compilacao de .NET Framework 4.8
- SQL Server LocalDB
- `sqlcmd` para preparar o banco

## Banco de dados

A connection string padrao aponta para `TesteEscola` na instancia `(localdb)\\MSSQLLocalDB`.

Crie/inicie a instancia e execute o script:

```powershell
sqllocaldb create MSSQLLocalDB -s
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i ".\script-banco.sql" -b
```

O script recria as tabelas `Aluno`, `Turma` e `Matricula` e insere dados de exemplo. Os arquivos fisicos ficam fora do repositorio, normalmente em `C:\Users\<usuario>\TesteEscola.mdf` e `TesteEscola_log.ldf`.

## Build e testes

```powershell
dotnet msbuild .\Evonautinhas.csproj /t:Build /p:Configuration=Debug
dotnet test .\Evonautinhas.Tests\Evonautinhas.Tests.csproj --configuration Debug
```

## Endpoints

```text
GET    /api/alunos?nome={filtro}&pagina={n}&tamanho={n}
GET    /api/alunos/{id}
POST   /api/alunos
PUT    /api/alunos/{id}
DELETE /api/alunos/{id}
GET    /api/turmas
POST   /api/matriculas
GET    /api/relatorios/alunos-por-turma
```

## UI e IIS Express

A UI fica dentro do proprio projeto e e servida pelo ASP.NET Web API. Inicie o projeto no IIS Express e acesse:

```text
http://localhost:{porta}/UI/index.html
```

O campo `API` fica vazio por padrao, portanto as chamadas usam a mesma origem (`/api/alunos`, `/api/relatorios/alunos-por-turma`). Isso evita a necessidade de configurar CORS. Para apontar para outra API durante desenvolvimento, informe uma URL absoluta no campo.

## Estrutura

```text
API/       Controllers, filtros e configuracao Web API
Business/  Servicos, regras, cache em memoria
Data/      Contexto SQL e repositories Dapper
Domain/    Entidades, contratos e excecoes
Database/  Schema versionado do banco
UI/        Interface HTML, CSS e jQuery
Evonautinhas.Tests/ Testes unitarios
```

## Estado do projeto

O cache Redis e uma alternativa futura; o cache ativo usa `MemoryCacheService`. A UI estatica esta pronta para consumir a API quando ela estiver hospedada via HTTP.
