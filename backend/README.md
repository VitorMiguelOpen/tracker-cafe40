# Backend — Café Tracker (.NET 10)

API REST + serviço MQTT Client, organizado em camadas seguindo **DDD**.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Acesso a um broker MQTT (ex.: Mosquitto local ou em container)

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

## Como rodar

```bash
cd backend
dotnet restore
dotnet run --project src/CafeTracker.Api
```

## Configuração

Copie `.env.example` (na raiz do repo) e ajuste as credenciais do broker MQTT e a connection string do banco. **Não** versione segredos — veja `.gitignore`.
