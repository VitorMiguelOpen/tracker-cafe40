# Backend — Café Tracker (.NET 10)

API REST + serviço MQTT Client, organizado em camadas seguindo **DDD**.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Acesso a um broker MQTT (ex.: Mosquitto local ou em container)

> **Banco de dados:** em **desenvolvimento** o backend usa **SQLite** (um arquivo
> local criado automaticamente), então **não é preciso instalar Docker nem PostgreSQL**
> para rodar e testar. Em **produção** usa-se PostgreSQL (ver seção *Configuração*).

## Estrutura sugerida (DDD)

```
backend/
├── src/
│   ├── CafeTracker.Domain/         # entidades, regras de negócio
│   ├── CafeTracker.Application/    # casos de uso
│   ├── CafeTracker.Infrastructure/ # MQTT client, repositórios, SignalR
│   └── CafeTracker.Api/            # endpoints REST + host
└── tests/
    └── CafeTracker.Tests/
```

## Como rodar (desenvolvimento — sem Docker/Postgres)

**Pela linha de comando:**

```bash
cd backend
dotnet restore
dotnet run --project src/CafeTracker.Api
```

**Pelo Visual Studio:** o repositório não tem `.sln`; abra o projeto diretamente em
**File → Open → Project/Solution** e selecione
`backend/src/CafeTracker.Api/CafeTracker.Api.csproj`. Escolha o perfil **`http`**
(porta 5000) e tecle **F5**.

Pronto. Na primeira execução o schema é criado num arquivo SQLite local
(`cafetracker.dev.db`, ignorado pelo Git). A API sobe em `http://localhost:5000`
e o serviço MQTT conecta no broker para ingerir as leituras da máquina.

> O dashboard (frontend) sobe em `http://localhost:8081` — suba o backend **antes**.

## Configuração

O provider de banco é escolhido por `Database:Provider`:

- **Desenvolvimento** (`appsettings.Development.json`): `Sqlite` — arquivo local, zero setup.
- **Produção** (`appsettings.json`): `Postgres` — usa `ConnectionStrings:Default`
  e aplica as migrações versionadas no startup. Para subir um Postgres local igual
  ao de produção, use o `docker-compose.yml` na raiz (`docker compose up -d`).

Credenciais sensíveis (broker MQTT, connection string de produção) vêm do `.env`
da raiz — copie `.env.example` e ajuste. **Não** versione segredos (veja `.gitignore`).
