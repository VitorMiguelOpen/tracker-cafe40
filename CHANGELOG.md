# Changelog

Todas as mudanças relevantes do Café Tracker. Formato baseado em
[Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/); versionamento
[SemVer](https://semver.org/lang/pt-BR/).

## [1.0.0] — 2026-06-08

Primeira versão de produção — implantada e acessível pelo navegador, com a máquina
**SAACE** sendo monitorada em tempo real.

### Funcionalidades
- **Status em tempo real (US-02):** indicador ligado/desligado com push via SignalR
  (atualiza em segundos após o evento MQTT).
- **Consumo por hora (US-03):** gráfico das 24 faixas horárias, com navegação por data.
- **Consumo diário e semanal (US-04):** consolidado do dia e da semana (seg→dom).
- **Horário de pico (US-05):** hora de maior uso do dia.
- **Tendência (US-06):** média diária e classificação (aumentando/estável/diminuindo).
- **Total de acionamentos do dia** (métrica derivada).
- **Dashboard responsivo:** layout adaptado para desktop e celular.

### Plataforma
- **Ingestão MQTT** do tópico `/IoT/SAACE/DADOSAPONTAMENTO` com reconexão automática.
- **Backend .NET 10** em arquitetura DDD, **servindo o dashboard SAP UI5 embutido**
  (aplicação inteira num único endereço).
- **Persistência:** SQLite em desenvolvimento (zero setup) e **PostgreSQL** em produção,
  com migrações aplicadas no startup.
- **Implantação via Docker (Caminho A):** stack `backend + PostgreSQL central` que sobe
  com um comando; dados **compartilhados** entre todos os acessos pelo navegador,
  coletando 24/7. Ver [`docs/DEPLOY.md`](docs/DEPLOY.md).
- **Configurável por ambiente:** porta do host (`APP_PORT`) e `MQTT_CLIENT_ID` via `.env`.
- **SAP UI5 e SignalR servidos localmente** (sem dependência de CDN em runtime).

### Correções notáveis durante o desenvolvimento
- Render do dashboard em branco corrigido (cadeia de altura do `ComponentContainer`).
- `DateTimeOffset` normalizado para UTC no PostgreSQL (`timestamptz`), destravando a
  ingestão e as consultas de consumo.
- Conexão MQTT/banco resilientes a credenciais ausentes em desenvolvimento.

[1.0.0]: https://github.com/VitorMiguelOpen/tracker-cafe40/releases/tag/v1.0.0
