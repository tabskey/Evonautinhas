# 🚀 Evonautinhas | School Enrollment API
<p align="center">
  <img src="./assets/logo.gif" alt="Evonautinhas" width="300">
</p>

> **Houston, temos uma matrícula.** 🧑‍🚀

> Uma API REST para gerenciamento de alunos, turmas e matrículas escolares.

Construída com **.NET Framework 4.8**, **ASP.NET Web API 2**, **Dapper** e **SQL Server**, com uma pequena interface web para controlar a missão.

---

## 🪐 A missão

O **Evonautinhas** é uma API de controle escolar responsável por cuidar de:

* 👩‍🚀 **Alunos** — cadastro, edição, inativação, busca e paginação
* 🛰️ **Turmas** — consulta de turmas e ocupação
* 🎫 **Matrículas** — validação de aluno, vagas disponíveis e duplicidade
* 📊 **Relatórios** — alunos agrupados por turma
* 🖥️ **Interface web** — uma UI simples para operar a missão sem precisar pilotar a API manualmente
* 🧪 **Testes** — cobertura das regras e comportamentos importantes com NUnit e Moq

---

## 🧰 Stack da nave

| Tecnologia              | Missão                       |
| ----------------------- | ---------------------------- |
| **.NET Framework 4.8**  | Motor da nave                |
| **ASP.NET Web API 2**   | Comunicação com a Terra      |
| **Dapper**              | Acesso ao banco              |
| **SQL Server**          | Centro de controle dos dados |
| **MemoryCache**         | Cache de turmas              |
| **jQuery + HTML + CSS** | Painel de controle           |
| **NUnit + Moq**         | Equipe de testes             |

> 🚧 **Redis:** previsto para uma futura missão. Por enquanto, o cache permanece em órbita usando `MemoryCacheService`.

---

## 👀 O painel de controle

A interface fica dentro do próprio projeto e inclui:

* 🔎 Busca e paginação de alunos
* ➕ Cadastro de alunos
* 🗃️ Arquivamento lógico de alunos, mantendo o registro com `Ativo = 0`
* ↩️ Reativação de alunos arquivados — aviso no cadastro quando o e-mail pertence a um aluno inativo
* 📝 Matrícula em turmas
* 📈 Visualização da ocupação das turmas
* 📊 Consulta de relatório de alunos por turma

A identidade visual usa **Poppins**, um astronauta animado e uma foto de perfil para dar ao projeto um pouco mais de vida.

---

## 🛸 Como colocar a nave no ar

### Pré-requisitos

Você vai precisar de:

* Windows
* Visual Studio 2022 **ou** .NET SDK com suporte à compilação de .NET Framework 4.8
* SQL Server LocalDB
* `sqlcmd`

### 1. Preparando a base de dados

A configuração padrão utiliza:

```text
Server:   (localdb)\MSSQLLocalDB
Database: TesteEscola
```

Crie e inicie a instância:

```powershell
sqllocaldb create MSSQLLocalDB -s
```

Depois, rode o script principal:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i ".\script-banco.sql" -b
```

O script cria as tabelas:

```text
Aluno
Turma
Matricula
```

e também envia alguns astronautas de teste para a missão. 👨‍🚀👩‍🚀

Quer mais tripulantes?

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d TesteEscola -E -i ".\Database\002-inject-more-alunos.sql" -b
```

Os arquivos físicos do banco ficam fora do repositório, normalmente em:

```text
C:\Users\<usuario>\TesteEscola.mdf
C:\Users\<usuario>\TesteEscola_log.ldf
```

---

## 🔧 Build & Test

Primeiro, verifique se a nave está funcionando:

```powershell
dotnet msbuild .\Evonautinhas.csproj /t:Build /p:Configuration=Debug
```

Depois, execute os testes:

```powershell
dotnet test .\Evonautinhas.Tests\Evonautinhas.Tests.csproj --configuration Debug
```

Se tudo estiver verde, podemos decolar. 🚀

---

## 📡 API

### 👩‍🚀 Alunos

```http
GET    /api/alunos?nome={filtro}&pagina={n}&tamanho={n}
GET    /api/alunos?nome={filtro}&incluirInativos=true&pagina={n}&tamanho={n}

GET    /api/alunos/{id}

POST   /api/alunos

PUT    /api/alunos/{id}

DELETE /api/alunos/{id}

POST   /api/alunos/{id}/reativar
```

### 🛰️ Turmas

```http
GET    /api/turmas
```

### 🎫 Matrículas

```http
POST   /api/matriculas
```

### 📊 Relatórios

```http
GET    /api/relatorios/alunos-por-turma
```

---

## 🖥️ Decolando a interface

A UI é servida pelo próprio ASP.NET Web API, então não existe uma segunda nave para configurar.

Compile o projeto:

```powershell
dotnet msbuild .\Evonautinhas.csproj /t:Build /p:Configuration=Debug
```

Inicie o IIS Express:

```powershell
& "C:\Program Files\IIS Express\iisexpress.exe" /path:"$PWD" /port:5000
```

Agora é só acessar:

```text
http://localhost:5000/UI/index.html
```

As chamadas utilizam a mesma origem:

```text
/api/alunos
/api/turmas
/api/matriculas
/api/relatorios/alunos-por-turma
```

Por isso, **não é necessário configurar CORS**.

Para desligar o IIS Express, pressione:

```text
Q
```

---

## 🗺️ Mapa da nave

```text
Evonautinhas/
│
├── API/                  # 👨‍🚀 Controllers, filtros e configuração Web API
├── Business/             # 🧠 Regras de negócio, serviços e cache
├── Data/                 # 🗄️ SQL + repositories Dapper
├── Domain/               # 📦 Entidades, contratos e exceções
├── Database/             # 🛠️ Scripts e versões do banco
│
├── UI/                   # 🖥️ Painel de controle
├── assets/               # 🚀 Logo, astronauta e identidade visual
│
├── Evonautinhas.Tests/   # 🧪 Testes unitários
│
├── Global.asax
├── Web.config
└── Evonautinhas.csproj
```

---

## 📋 Status da missão

| Área                   |      Status     |
| ---------------------- | :-------------: |
| 👩‍🚀 CRUD de alunos   |        ✅        |
| 🛰️ Consulta de turmas |        ✅        |
| 🎫 Matrículas          |        ✅        |
| 📊 Relatórios          |        ✅        |
| 🖥️ Interface web      |        ✅        |
| 🧪 Testes unitários    |        ✅        |
| ⚡ Memory Cache         |        ✅        |
| 🔴 Redis               |    🛸 Futuro    |
| 🌌 Conquistar Marte    | 🔭 Investigando |

---

## 🌙 Próxima missão

O projeto atualmente utiliza `MemoryCacheService` para o cache de turmas.

Uma evolução natural seria substituir ou complementar esse mecanismo com **Redis**, permitindo compartilhar o cache entre múltiplas instâncias da API.

Até lá...

> **Keep calm and trust the astronaut.** 🚀
