# Frontend — Café Tracker (SAP UI5)

Dashboard web responsivo que consome a API .NET e recebe atualizações em tempo real via SignalR.

## Pré-requisitos

- [Node.js](https://nodejs.org) (LTS)
- [UI5 Tooling](https://sap.github.io/ui5-tooling/) — instalado como dependência de dev (`@ui5/cli`)

## Como rodar

> O **backend precisa estar rodando** em `http://localhost:5000` (ver `backend/`),
> com o Postgres no ar (`docker compose up -d` na raiz).

```bash
cd frontend
npm install      # baixa o @ui5/cli e o framework OpenUI5
npm start        # sobe em http://localhost:8080 (ui5 serve)
```

## Configuração

- **Endereço do backend:** `webapp/Config.js` (padrão `http://localhost:5000`).
- O backend já libera CORS para `http://localhost:8080` e `http://localhost:5173`
  (ver `Cors:Origins` no `appsettings.json`). Se mudar a porta do front, ajuste lá.

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
