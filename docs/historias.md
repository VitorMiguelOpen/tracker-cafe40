# Histórias de usuário (backlog)

Backlog **must have** do Café Tracker (US-01 a US-07). Cada história traz o objetivo e os **critérios de aceite** — alinhados com os pontos de validação da QA, para que desenvolvimento e teste partam do mesmo entendimento.

> Escopo funcional completo em [escopo.md](escopo.md). Funcionalidades *nice to have* (responsividade, alertas, estimativa de energia, previsão de pico, vídeos) ficam após o fechamento das must have.

---

## US-01 — Conexão e recepção de dados MQTT
**Como** sistema, **quero** receber e persistir os eventos do sensor via MQTT **para** ter a base de dados do dashboard.

Critérios de aceite:
- [ ] Backend conecta no broker e assina o tópico da máquina (SAACE).
- [ ] Eventos de status (ligado/desligado) são persistidos no banco.
- [ ] O timestamp salvo corresponde ao horário real do evento.
- [ ] Reconexão automática após perda de conexão MQTT.
- [ ] Nenhum dado é perdido durante a reconexão.

## US-02 — Indicador visual de status em tempo real
**Como** usuário, **quero** ver o status atual do equipamento **para** saber se está ligado ou desligado na hora.

Critérios de aceite:
- [ ] Indicador muda de cor: verde = ligado, vermelho = desligado.
- [ ] Texto correspondente ao status é exibido.
- [ ] Atualização sem reload da página.
- [ ] Atualização em **até 3 segundos** após o evento do sensor.
- [ ] O último evento define o status atual.

## US-03 — Gráfico de consumo por hora
**Como** usuário, **quero** ver o uso agrupado por hora **para** entender a distribuição ao longo do dia.

Critérios de aceite:
- [ ] Exibe as 24 horas do dia.
- [ ] Agrupamento correto por hora (tempo ligado e/ou acionamentos).
- [ ] Navegação entre dias anteriores.
- [ ] Consistência entre o gráfico e os dados do banco.

## US-04 — Consumo diário e semanal
**Como** usuário, **quero** visualizar consumo por dia e por semana **para** comparar períodos.

Critérios de aceite:
- [ ] Visão diária funcional.
- [ ] Visão semanal funcional.
- [ ] Navegação entre períodos anteriores.
- [ ] Sem falhas visuais ou telas em branco (inclusive em períodos sem dados ou com poucos dados).

## US-05 — Horário de pico
**Como** usuário, **quero** identificar o horário de maior uso **para** planejar melhor a operação.

Critérios de aceite:
- [ ] Identifica automaticamente o horário de maior uso.
- [ ] Cálculo considera o **maior tempo acumulado** de uso no período.
- [ ] Destaque visual claro no dashboard.

## US-06 — Tendências e médias
**Como** usuário, **quero** ver médias e a tendência de uso **para** entender o comportamento ao longo do tempo.

Critérios de aceite:
- [ ] Média diária de uso calculada corretamente.
- [ ] Tendência operacional: crescimento / estabilidade / redução.
- [ ] Cálculos baseados no histórico persistido.

## US-07 — Total de acionamentos do dia
**Como** usuário, **quero** ver quantas vezes o equipamento foi acionado hoje **para** medir o uso.

Critérios de aceite:
- [ ] Cada transição válida **desligado → ligado** conta como 1 acionamento.
- [ ] Total atualiza em tempo real.
- [ ] Sem contagens duplicadas.
- [ ] Destaque visual no dashboard.

---

## Fluxo entre desenvolvimento e QA

Acordo de "pronto para teste" (a alinhar com a QA — Juliana):
- Uma história só vai para validação quando os critérios de aceite acima estiverem implementados e rodando localmente.
- A QA prepara **massa de testes simulada** (ex.: equipamento ligado por 3h, múltiplos acionamentos, dias com uso intenso/baixo) para validar gráficos e indicadores.
- Bugs são reportados no template padrão da QA (título, passos, esperado vs. atual, evidências, ambiente).
