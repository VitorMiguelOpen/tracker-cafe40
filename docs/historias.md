# Histórias de usuário (backlog)

Backlog **must have** do Café Tracker, alinhado ao documento oficial *"Histórias de Usuário — Café 4.0"* (6 histórias: US-01 a US-06). Cada história traz o objetivo e os **critérios de aceite**, espelhando os cenários de teste do documento oficial, para que desenvolvimento e QA partam do mesmo entendimento.

> Escopo funcional completo em [escopo.md](escopo.md). Funcionalidades *nice to have* (responsividade, alertas, estimativa de energia, previsão de pico, vídeos) ficam após o fechamento das must have.

---

## US-01 — Conexão e recepção de dados MQTT
**Como** sistema, **quero** receber, processar e persistir os eventos do sensor via MQTT **para** ter a base de dados confiável do dashboard, mesmo após falhas de conexão.

Critérios de aceite:
- [ ] Backend conecta no broker e **permanece inscrito** no tópico da máquina (SAACE).
- [ ] Eventos de status (ligado/desligado) são persistidos no banco.
- [ ] O timestamp salvo corresponde ao **horário real** do evento.
- [ ] **Ordem cronológica** dos eventos é mantida na persistência.
- [ ] **Reconexão automática** após perda de conexão, sem intervenção manual.
- [ ] A reconexão **não gera duplicidade** de eventos no banco.
- [ ] O sistema armazena: status do sensor, timestamp do evento e identificação do dispositivo/sensor.

Cenários de teste (do doc oficial):
- Recebimento de evento "ligado".
- Persistência do status no banco após processamento.
- Sequência ligado → desligado → ligado novamente, com horários corretos.
- Reconexão automática após queda do broker.
- Persistência continua após o restabelecimento da conexão.

> **Ponto em aberto (a validar):** o critério "nenhum evento perdido durante a reconexão" depende do nível de QoS e do tipo de sessão MQTT. Hoje a ingestão usa **QoS 0 + clean session** (alinhado ao projeto de referência). Garantir entrega no período offline exigiria **QoS 1 + sessão persistente** — a confirmar com a configuração real do sensor/broker antes de alterar.

## US-02 — Indicador visual de status em tempo real
**Como** operador/gestor, **quero** ver o status atual do equipamento por um indicador visual e textual **para** saber se está ligado ou desligado na hora.

Critérios de aceite:
- [ ] Indicador muda de cor: **verde = ligado**, **vermelho = desligado**.
- [ ] Texto correspondente ao status é exibido e **sincronizado** com a cor.
- [ ] Atualização **automática**, sem reload da página.
- [ ] Atualização em **até 3 segundos** após o evento do sensor.
- [ ] **Monitoramento contínuo**: com o dashboard aberto por longos períodos, segue refletindo as mudanças em tempo real.
- [ ] O último evento define o status atual.

> Recomendação técnica do doc oficial: usar mecanismo de tempo real (WebSocket, **SignalR** ou equivalente). Esta história depende da correta recepção dos eventos da US-01.

## US-03 — Gráfico de consumo por hora
**Como** usuário, **quero** ver a quantidade de acionamentos do equipamento agrupada por hora **para** entender a distribuição de uso ao longo do dia.

Critérios de aceite:
- [ ] Exibe **as 24 horas do dia** (00h a 23h).
- [ ] Agrupa corretamente os eventos por hora e mostra a quantidade em cada período.
- [ ] Múltiplos acionamentos na mesma hora são **consolidados** no mesmo agrupamento.
- [ ] **Navegação entre datas**; mostra apenas os registros do dia selecionado.
- [ ] Hora **sem registro** exibe **valor zero**.
- [ ] Consistência entre o gráfico e os dados do banco.
- [ ] Agrupamento respeita o **timezone** configurado na aplicação.

Cenários de teste (do doc oficial):
- Equipamento ligado às 08:15, 12:30 e 18:45 → consumos nas horas 08h, 12h e 18h.
- Acionamentos às 14:05, 14:20, 14:35 e 14:50 → 4 eventos agrupados na hora 14h.
- Comparação dos valores do gráfico com os dados brutos do banco.
- Navegação entre dias distintos sem misturar registros.

### Métrica derivada — Total de acionamentos do dia
Complemento direto da US-03 (não é história separada do doc oficial). Expõe o número total de acionamentos do dia, útil para o destaque do dashboard e como insumo da US-05/US-06.
- [ ] Cada transição válida **desligado → ligado** conta como 1 acionamento.
- [ ] Total atualiza em tempo real.
- [ ] Sem contagens duplicadas (reaproveita a regra de transição da US-01).

## US-04 — Consumo diário e semanal
**Como** gestor/supervisor, **quero** visualizar o consumo por dia e por semana, navegando entre períodos **para** comparar o comportamento ao longo do tempo.

Critérios de aceite:
- [ ] Visão **diária** funcional.
- [ ] Visão **semanal** (dados consolidados) funcional.
- [ ] **Navegação entre períodos** sem erros ou inconsistências.
- [ ] Período **com dados**: exibe de forma correta e consistente.
- [ ] Período **sem dados**: mensagem informativa, **sem tela em branco** nem falha visual.
- [ ] Período **com poucos dados**: exibe corretamente, mantendo a integridade visual.
- [ ] Visões diária e semanal usam a **mesma fonte de dados** dos registros do banco.

## US-05 — Horário de pico de utilização
**Como** gestor/supervisor, **quero** identificar automaticamente o horário de maior uso, com destaque no dashboard **para** planejar melhor a operação.

Critérios de aceite:
- [ ] Identifica automaticamente o horário de **maior tempo acumulado** de funcionamento.
- [ ] Soma corretamente o tempo de utilização de cada faixa horária.
- [ ] **Destaque visual** claro e consistente em todas as visões.
- [ ] Atualiza automaticamente conforme novos dados são processados.

Validação com massa conhecida (do doc oficial): 08h = 30 min, 10h = 2h, 14h = 1h → pico identificado às **10h**.

> **Regra de desempate (a validar com negócio):** em caso de empate entre horários com o mesmo tempo acumulado, a regra de desempate será definida pelo produto. Por ora, sugestão de critério provisório: o horário mais cedo.

## US-06 — Média diária e tendência de consumo
**Como** usuário, **quero** ver a média diária e a tendência de uso **para** entender o comportamento ao longo do tempo.

Critérios de aceite:
- [ ] **Média diária** calculada com base nos registros do período selecionado (apenas dados válidos).
- [ ] Tendência classificada em um dos 3 estados: **Aumentando / Estável / Diminuindo**.
- [ ] Tendência baseada na comparação dos consumos ao longo do histórico disponível.
- [ ] Histórico **parcial/reduzido** é tratado sem erros ou inconsistências.
- [ ] Cálculos atualizam automaticamente quando novos dados entram no histórico.
- [ ] Ausência de dados não causa falha de exibição.

Cenários de validação (do doc oficial):
- Crescimento gradual (1h, 2h, 3h, 4h) → **Aumentando**.
- Redução gradual (4h, 3h, 2h, 1h) → **Diminuindo**.
- Estabilidade (2h, 2h, 2h, 2h) → **Estável**.

---

## Fluxo entre desenvolvimento e QA

Acordo de "pronto para teste" (a alinhar com a QA — Juliana):
- Uma história só vai para validação quando os critérios de aceite acima estiverem implementados e rodando localmente.
- A QA prepara **massa de testes simulada** (ex.: equipamento ligado por 3h, múltiplos acionamentos, dias com uso intenso/baixo, crescimento/redução/estabilidade) para validar gráficos e indicadores.
- Validação da US-01 inclui simular: sensor ligado, desligado, ligado novamente; queda e restabelecimento de conexão MQTT; conferência direta no banco.
- Bugs são reportados no template padrão da QA (título, passos, esperado vs. atual, evidências, ambiente).
- **Evidências de teste documentadas** para cada história (requisito da "Definição de Pronto" do doc oficial).
