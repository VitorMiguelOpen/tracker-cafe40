# Deploy com Docker (ambiente compartilhado — Caminho A)

Sobe a aplicação inteira (backend + dashboard + PostgreSQL) com **um comando**, num
endereço único que todos acessam pelo navegador. Não precisa instalar .NET nem Node
no host — só **Docker**.

## Pré-requisitos
- **Docker Desktop** (Windows/Mac) ou **Docker Engine + Compose** (Linux).
- Arquivo **`.env`** na raiz (copie de `.env.example`) com as credenciais do MQTT e a
  senha do Postgres:
  ```env
  MQTT_USERNAME=...        # credenciais do broker (para dado ao vivo)
  MQTT_PASSWORD=...
  POSTGRES_PASSWORD=troque-por-uma-senha-forte
  ```
  > O `.env` **não** é versionado. As credenciais ficam só no host.

## Rodar
```bash
docker compose up -d --build      # builda e sobe tudo (1ª vez baixa o SAP UI5)
```
Abra **http://localhost:5000** (no servidor: `http://<host>:5000`).

Comandos úteis:
```bash
docker compose logs -f app        # acompanhar o backend (MQTT, etc.)
docker compose down               # parar (mantém os dados no volume)
docker compose down -v            # parar e APAGAR os dados
```

## Levar para um servidor
1. Tenha **Docker** no servidor (VM da empresa ou nuvem).
2. Copie o projeto (ou `git clone`) e crie o `.env` lá.
3. `docker compose up -d --build`.
4. Libere a **porta 5000** para a rede e divulgue o endereço (`http://<host>:5000`
   ou um DNS interno).

### Observações
- **Dados centralizados e persistentes:** ficam no PostgreSQL (volume `cafetracker-pgdata`).
  Como o backend fica sempre ligado, não há lacuna na coleta MQTT.
- **HTTPS / domínio:** para acesso além da rede interna, coloque um proxy reverso
  (Caddy/nginx) na frente, com TLS.
- **Versão das imagens .NET:** o Dockerfile usa `sdk:10.0` / `aspnet:10.0`. Se a tag
  não existir no seu ambiente, ajuste para a tag de .NET 10 disponível.
- **Desenvolvimento** continua simples e sem Docker: `dotnet run` (SQLite local) — ver
  `backend/README.md`.
