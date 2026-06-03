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

**Pelo Visual Studio (recomendado — sobe front + API juntos):**

> Requer **Visual Studio 2026** ou mais recente — o **VS 2022 não suporta .NET 10**.
> Sem ele, use a opção por linha de comando abaixo (`dotnet run`).

1. Abra `backend/CafeTracker.slnx`.
2. Tecle **F5** (perfil `http`, porta 5000).
3. Abre em `http://localhost:5000` com o **dashboard completo**.

A API **serve o frontend embutido** (pasta `wwwroot`, gerada no build). Na **primeira**
compilação, o build do .NET também constrói o frontend e baixa o SAP UI5 (alguns
minutos, só na 1ª vez; precisa de **Node.js** instalado e internet). Para reconstruir
o front, apague `src/CafeTracker.Api/wwwroot`. Para compilar só o backend, use
`dotnet build -p:SkipFrontend=true`.

**Pela linha de comando:**

```bash
cd backend
dotnet restore
dotnet run --project src/CafeTracker.Api   # build do front embutido + API em :5000
```

Na primeira execução o schema é criado num arquivo SQLite local
(`cafetracker.dev.db`, ignorado pelo Git) e o serviço MQTT conecta no broker.

## Configuração

O provider de banco é escolhido por `Database:Provider`:

- **Desenvolvimento** (`appsettings.Development.json`): `Sqlite` — arquivo local, zero setup.
- **Produção** (`appsettings.json`): `Postgres` — usa `ConnectionStrings:Default`
  e aplica as migrações versionadas no startup. Para subir um Postgres local igual
  ao de produção, use o `docker-compose.yml` na raiz (`docker compose up -d`).

Credenciais sensíveis (broker MQTT, connection string de produção) vêm do `.env`
da raiz — copie `.env.example` e ajuste. **Não** versione segredos (veja `.gitignore`).
