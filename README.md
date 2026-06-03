# Café Tracker — Equipe Café 4.0

> Monitoramento inteligente de equipamento industrial em tempo real, a partir de um sensor IoT que publica status (ligado/desligado) via **MQTT**.

Projeto desenvolvido no hackathon **HACK the OPEN — Desafio Café Tracker** (OPEN Solutions · SAP Partner).

---

## Visão geral

O cliente possui uma máquina com um sensor IoT que já publica o status (ligado/desligado) via MQTT, mas **não tem nenhuma visualização ou análise** desses dados.

> _"Eu estou cego quanto ao uso desse equipamento."_ — Sandro (cliente)

O Café Tracker transforma esse dado bruto em informação útil: um dashboard moderno, em tempo real, com gráficos de consumo, identificação de horário de pico e indicadores de tendência para apoio à tomada de decisão.

## Stack

| Camada        | Tecnologia            | Papel                                            |
| ------------- | --------------------- | ------------------------------------------------ |
| Frontend      | **SAP UI5**           | Dashboard web responsivo                         |
| Backend       | **.NET 10**           | API REST + serviço MQTT Client                   |
| Mensageria    | **MQTT**              | Recepção dos eventos do sensor                   |
| Tempo real    | **SignalR / WebSocket** | Push de atualização para o dashboard           |
| Persistência  | Banco de dados        | Histórico de eventos para análise                |
| Arquitetura   | **DDD**               | Domain-Driven Design no backend                  |

## Funcionalidades

**Must have**
- Indicador visual de status em tempo real (verde = ligado, vermelho = desligado)
- Total de acionamentos do dia
- Gráfico de consumo por hora
- Gráficos de consumo diário e semanal
- Identificação automática de horário de pico
- Indicadores de tendência (crescimento / estabilidade / redução)
- Atualização do dashboard em até 3 segundos após o evento MQTT

**Nice to have**
- Layout responsivo (celular, tablet, TV, monitor)
- Alertas e notificações de consumo
- Estimativa de consumo de energia
- Previsão de pico (modelo preditivo)
- Vídeo demonstrativo / material de onboarding

## Estrutura do repositório

```
tracker-cafe40/
├── backend/      # API .NET 10 + serviço MQTT (DDD)
├── frontend/     # Dashboard SAP UI5
├── docs/         # Escopo, ata de kickoff, arquitetura e MQTT
└── README.md
```

## Como executar

> Instruções detalhadas em [`backend/README.md`](backend/README.md) e
> [`frontend/README.md`](frontend/README.md).

Em **desenvolvimento não é preciso instalar Docker nem PostgreSQL**: o backend usa
um banco **SQLite** local, criado automaticamente no primeiro start.

### Opção 1 — Tudo junto (recomendado): abrir e dar F5

A API **serve o dashboard embutido**, então a aplicação inteira (frontend + API)
sobe num único endereço. Basta o **Visual Studio**:

1. Abra `backend/CafeTracker.slnx`.
2. Tecle **F5**.
3. Abre em **http://localhost:5000** com o dashboard completo.

> **Versão do Visual Studio:** o projeto usa **.NET 10**, que **não é suportado pelo
> Visual Studio 2022** — abrir/buildar pela IDE exige o **Visual Studio 2026** (ou
> mais recente). Se você só tem o 2022, use a **Opção 2** (linha de comando
> `dotnet run`), que funciona com qualquer versão.

Na **primeira** compilação, o Visual Studio também constrói o frontend e baixa o
SAP UI5 (alguns minutos, só na 1ª vez; precisa de internet). Depois é instantâneo.
Requer **Node.js** instalado (o build do frontend é disparado pelo build do .NET).

### Opção 2 — Frontend em separado (para desenvolver o front com live-reload)

```bash
# Backend (.NET 10) — API em http://localhost:5000
cd backend
dotnet run --project src/CafeTracker.Api

# Frontend (SAP UI5) — dashboard em http://localhost:8081
cd frontend
npm install
npm start
```

Em **produção** o backend usa **PostgreSQL** (ver `backend/README.md`); o
`docker-compose.yml` na raiz sobe um Postgres local idêntico, se você quiser usá-lo.

## Documentação

- [Histórias de usuário (backlog US-01 a US-07)](docs/historias.md)
- [Escopo do projeto](docs/escopo.md)
- [Ata de kickoff com o cliente](docs/ata-kickoff.md)
- [Arquitetura](docs/arquitetura.md)
- [MQTT — conceitos e conexão](docs/mqtt.md)
- [Dados do sensor via MQTT (estrutura real)](docs/dados-mqtt.md)
- [Plano de ingestão MQTT (backend)](docs/ingestao-mqtt.md)

## Equipe

Time **Café 4.0** — Líder: Ana Clara · Papéis: Líder, Dev, QA, Produto.

## Licença

[MIT](LICENSE)
