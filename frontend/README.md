# Frontend — Café Tracker (SAP UI5)

Dashboard web responsivo que consome a API .NET e recebe atualizações em tempo real via SignalR.

## Pré-requisitos

- [Node.js](https://nodejs.org) (LTS)
- [UI5 Tooling](https://sap.github.io/ui5-tooling/) — instalado como dependência de dev (`@ui5/cli`)

## Como rodar

> O **backend precisa estar rodando** em `http://localhost:5000` (ver `backend/`).
> Em desenvolvimento o backend usa SQLite, então **não é preciso Docker/Postgres**.

```bash
cd frontend
npm install      # baixa o @ui5/cli
npm start        # sobe em http://localhost:8081 (ui5 serve)
```

> **Primeira execução:** ao rodar `npm start` pela primeira vez, o UI5 Tooling baixa
> o SAPUI5 (libs declaradas no `ui5.yaml`, incl. `sap.viz`) para um cache local em
> `~/.ui5`. Isso pode levar 1–2 minutos e exige internet **só nessa primeira vez**.

O SAP UI5 é servido **localmente** (em `/resources`, a partir do cache do UI5 Tooling)
— ver `index.html` (bootstrap em `resources/sap-ui-core.js`). Não há dependência de
CDN em runtime, o que faz o app funcionar em redes corporativas/offline. Usamos o
framework **SAPUI5** (não OpenUI5) porque os gráficos (`sap.viz`) só existem no SAPUI5.
O cliente SignalR também é servido localmente (`webapp/lib/signalr.min.js`).

## Configuração

- **Endereço do backend:** `webapp/Config.js` (padrão `http://localhost:5000`).
- **Porta do front:** `8081` (definida em `package.json`, script `start`). O backend
  libera CORS para `http://localhost:8081`, `http://localhost:8080` e
  `http://localhost:5173` (ver `Cors:Origins` no `appsettings.json`). Se mudar a porta
  do front, adicione-a também no CORS.

## O que entra aqui

- **US-02** — Status em tempo real (indicador verde/vermelho + texto), via SignalR.
- **US-03** — Gráfico de consumo por hora (24h) com navegação de data.
- **US-04** — Consumo diário e semanal (alternância Diário/Semanal).
- **US-05** — Card de horário de pico.
- **US-06** — Card de tendência (Aumentando/Estável/Diminuindo) + média diária.
- Métrica derivada — Total de acionamentos do dia.

## Estrutura

```
webapp/
  index.html              bootstrap do OpenUI5 + cliente SignalR (CDN)
  Component.js            componente raiz
  manifest.json           descritor do app (libs, rootView, modelos)
  Config.js               endereço do backend
  controller/Dashboard.controller.js   carga de dados (REST) + tempo real
  view/Dashboard.view.xml              telas (status, KPIs, gráficos)
  service/RealtimeService.js           wrapper do SignalR
  model/formatter.js                   formatadores (duração, status, tendência)
  i18n/i18n.properties                 textos
  css/style.css                        ajustes visuais
```
