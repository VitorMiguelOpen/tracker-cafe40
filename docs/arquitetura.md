# Arquitetura

## Visão de alto nível

```
┌──────────┐   MQTT    ┌─────────────────────────────┐   SignalR/WS   ┌──────────────┐
│  Sensor  │ ────────▶ │      Backend .NET 10         │ ─────────────▶ │  SAP UI5     │
│  IoT     │  publish  │  MQTT Client → Domínio → API │   push/REST    │  Dashboard   │
└──────────┘           └──────────────┬──────────────┘                └──────────────┘
                                       │
                                       ▼
                               ┌──────────────┐
                               │ Banco de dados│  (histórico de eventos)
                               └──────────────┘
```

## Fluxo de dados

1. O sensor publica o status (ligado/desligado) em um tópico MQTT.
2. O `MQTT Client` no backend assina o tópico e recebe cada evento.
3. O evento é validado, transformado em um evento de domínio e **persistido** com timestamp.
4. O backend calcula/atualiza os indicadores (acionamentos, consumo por hora, pico, tendência).
5. A atualização é empurrada para o dashboard via SignalR/WebSocket em tempo real.
6. O dashboard também consome a API REST para consultas históricas e navegação por período.

## Camadas (DDD)

Organização sugerida do backend seguindo Domain-Driven Design:

- **Domain** — entidades, value objects, regras de negócio (ex.: o que conta como um acionamento, cálculo de pico). Sem dependências de infraestrutura.
- **Application** — casos de uso / serviços de aplicação que orquestram o domínio (ex.: processar evento recebido, montar consumo por hora).
- **Infrastructure** — implementações concretas: cliente MQTT, repositórios (banco), hub SignalR.
- **Api (Presentation)** — endpoints REST e configuração de DI/hosting.

> A regra central do domínio: **cada transição válida `desligado → ligado` é um acionamento**, e o **último evento define o status atual**.

## Decisões em aberto

- Broker MQTT a utilizar (Mosquitto local/container para testes).
- Banco de dados (relacional para histórico + consultas analíticas).
- Convenção de tópicos: `area/equipamento/grandeza` (ex.: `fabrica/linha1/cafeteira/status`).
- QoS por tipo de mensagem (status pode usar QoS 1 + `retain` para o último estado).
