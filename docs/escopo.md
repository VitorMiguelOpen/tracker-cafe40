# Escopo do projeto — Monitoramento inteligente de equipamento via MQTT

## Visão geral

Solução de monitoramento em tempo real de equipamento industrial usando comunicação MQTT, backend em .NET e dashboard web em SAP UI5. Permite acompanhar o status operacional, analisar padrões de consumo e fornecer indicadores para apoio à decisão.

Composição:
- Integração com sensor via MQTT
- Backend .NET para recepção e processamento dos dados
- Banco de dados para persistência histórica
- Dashboard web com atualização em tempo real
- Indicadores analíticos e gráficos gerenciais

## Objetivo do produto

Plataforma simples, visual e confiável para monitorar o uso do equipamento, permitindo:
- Acompanhamento em tempo real
- Análise histórica de consumo
- Identificação de horários de pico
- Visualização de tendências de utilização
- Apoio à tomada de decisão operacional

## Escopo funcional

### 1. Integração com sensor MQTT
- Conexão do backend .NET ao broker MQTT
- Recepção contínua de mensagens do sensor
- Persistência dos eventos recebidos no banco
- Reconexão automática em caso de perda de conexão
- Registro de timestamp dos eventos

**Dados recebidos:** status do equipamento (ligado/desligado) + data/hora do evento.

**Regras de negócio:**
- Todo evento recebido deve ser armazenado
- O sistema deve suportar reconexão automática
- Timestamps devem respeitar o horário do servidor

### 2. Dashboard de status em tempo real
- Indicador visual dinâmico (verde = ligado, vermelho = desligado)
- Atualização automática sem reload da página
- Exibição textual do status
- Atualização em até 3 segundos após o evento MQTT

**Regras:** o último evento define o status atual; o dashboard permanece sincronizado com o sensor.

### 3. Dashboard analítico de consumo

**3.1 Consumo por hora** — 24 horas do dia, uso agrupado por hora, navegação entre dias. Métrica: tempo ligado por hora ou quantidade de acionamentos por hora.

**3.2 Consumo diário e semanal** — visualização diária e semanal, navegação entre períodos, comparativo entre períodos.

### 4. Indicadores inteligentes

**4.1 Horário de pico** — identificação automática do horário de maior uso (maior tempo acumulado no período), com destaque visual.

**4.2 Tendência** — média diária de uso e tendência operacional (crescimento / estabilidade / redução), com base no histórico.

**4.3 Total de acionamentos do dia** — em tempo real. Cada transição válida `desligado → ligado` conta como um acionamento. *Métrica derivada do consumo por hora (não é história separada no documento oficial); ver [historias.md](historias.md), US-03.*

## Escopo técnico

- **Backend:** .NET, serviço MQTT Client, API REST, processamento de eventos em tempo real.
- **Frontend:** dashboard web responsivo, atualização em tempo real via WebSocket/SignalR, gráficos interativos.
- **Banco:** persistência histórica de eventos (status + timestamps), estrutura preparada para consultas analíticas.

## Requisitos não funcionais

- **Performance:** atualização em até 3 s; persistência sem perda de eventos.
- **Disponibilidade:** reconexão automática com o broker; tratamento de falhas.
- **Usabilidade:** dashboard intuitivo, indicadores simples, fácil para usuários não técnicos.
- **Escalabilidade:** estrutura preparada para múltiplos equipamentos no futuro.

## Critérios de aceite

- Todas as histórias *must have* implementadas
- Dashboard atualizado em tempo real
- Dados persistidos corretamente
- Indicadores e gráficos validados pelo negócio
- Navegação funcional e intuitiva
- Código versionado no repositório
- Fluxo ponta a ponta demonstrado em ambiente funcional

## Premissas

- O sensor MQTT já estará configurado e enviando dados
- O broker MQTT estará acessível pela aplicação
- Conectividade estável entre sensor e servidor
- O equipamento enviará status confiáveis

## Riscos

- Instabilidade da conexão MQTT
- Perda de comunicação do sensor
- Dados inconsistentes enviados pelo hardware
- Latência de rede impactando o tempo real

## Entregáveis

Backend .NET funcional · banco estruturado · dashboard web operacional · integração MQTT ativa · indicadores e gráficos · repositório versionado · ambiente de demonstração funcional.
