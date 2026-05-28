# Frontend — Café Tracker (SAP UI5)

Dashboard web responsivo que consome a API .NET e recebe atualizações em tempo real via SignalR/WebSocket.

## Pré-requisitos

- [Node.js](https://nodejs.org) (LTS)
- [UI5 Tooling](https://sap.github.io/ui5-tooling/) (`npm install --global @ui5/cli`)

## Como rodar

```bash
cd frontend
npm install
npm start        # ou: ui5 serve
```

## O que entra aqui

- Tela de status em tempo real (indicador verde/vermelho)
- Gráficos de consumo por hora, diário e semanal
- Cards de indicadores (acionamentos do dia, horário de pico, tendência)
- Conexão com o backend (REST + tempo real)
