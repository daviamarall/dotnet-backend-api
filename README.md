# dotnet-backend-api

[![CI](https://github.com/daviamarall/dotnet-backend-api/actions/workflows/ci.yml/badge.svg)](https://github.com/daviamarall/dotnet-backend-api/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-10-512BD4?logo=.net&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

API REST de exemplo em **.NET 10** (Minimal APIs), com **Entity Framework Core (SQLite)**, **testes de integração** de ponta a ponta e pipeline de **CI** com build, testes e imagem Docker.

> Projeto de portfólio focado em **desenvolvimento backend**.

---

## 🎯 O que este repositório demonstra

- **Minimal APIs** organizadas com `MapGroup` e endpoints tipados
- **Entity Framework Core** com SQLite (`EnsureCreated` para simplicidade)
- **Camadas separadas**: `Models`, `Data`, configuração na composição (Program)
- **OpenAPI** nativo (`/openapi/v1.json`) para documentação da API
- **Health checks** (`/healthz`) com verificação do banco (`AddDbContextCheck`)
- **Testes de integração** com `WebApplicationFactory` + **EF Core InMemory** (xUnit)
- **Dockerfile multi-stage** rodando como usuário **não-root**
- **CI** no GitHub Actions: restore → build → test → build da imagem

## 🏗️ Estrutura

```
dotnet-backend-api/
├── src/Api/
│   ├── Program.cs            # composição + endpoints (Minimal API)
│   ├── Models/Product.cs     # entidade
│   ├── Data/AppDbContext.cs  # DbContext (EF Core)
│   └── appsettings.json
├── tests/Api.Tests/          # testes de integração (xUnit)
├── Dockerfile
├── docker-compose.yml
└── .github/workflows/ci.yml
```

## 🚀 Como rodar

### Local (.NET)

```bash
dotnet run --project src/Api
# API em http://localhost:5xxx  |  OpenAPI em /openapi/v1.json
```

### Docker

```bash
docker compose up --build
# API em http://localhost:8080
```

### Testes

```bash
dotnet test
```

## 🔌 Endpoints

| Método | Rota             | Descrição                    |
|--------|------------------|------------------------------|
| GET    | `/`              | Metadados do serviço         |
| GET    | `/healthz`       | Health check (inclui o banco)|
| GET    | `/openapi/v1.json` | Especificação OpenAPI      |
| GET    | `/products`      | Lista produtos               |
| GET    | `/products/{id}` | Busca produto por id         |
| POST   | `/products`      | Cria produto                 |
| PUT    | `/products/{id}` | Atualiza produto             |
| DELETE | `/products/{id}` | Remove produto               |

### Exemplo

```bash
curl -X POST http://localhost:8080/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Keyboard","price":199.90,"stock":10}'
```

## 🧩 Pipeline (`ci.yml`)

| Job          | O que faz                                  |
|--------------|--------------------------------------------|
| `build-test` | `dotnet restore` → `build` → `test`        |
| `docker`     | Build da imagem (sem push)                 |

## 🛠️ Stack

.NET 10 · Minimal APIs · EF Core 10 · SQLite · OpenAPI · xUnit · Docker · GitHub Actions

## 📄 Licença

MIT — veja [LICENSE](LICENSE).
