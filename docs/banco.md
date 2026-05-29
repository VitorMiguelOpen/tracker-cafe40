# Banco de dados — fluxo e modelo (proposta)

> Proposta de design para revisão. **Ainda sem código/migrations** — é o fluxo e o esquema para o time discutir e ajustar.
> Baseada em tudo que mapeamos: status `H1`/tag 99 (`1` = fazendo café, `0` = parado), fonte de eventos `DADOSAPONTAMENTO`, e o fato de o valor **se repetir** (publicado periodicamente, não só na mudança).

## Fluxo do dado (da máquina ao dashboard)

```
MQTT  /IoT/SAACE/DADOSAPONTAMENTO
   │  payload: "2026-05-28T16:44:08.000-03:00|99|1|Status (H1)|Machine status signal|"
   ▼
[Parser]  → { machineCode: "SAACE", tag: 99, value: 1, eventTime: 2026-05-28T16:44:08-03:00 }
   ▼
[Regra de transição]  compara value com o ÚLTIMO valor conhecido da máquina
   │
   ├─ valor NÃO mudou  ─▶ ignora (evita duplicado — o valor se repete)
   │
   └─ valor MUDOU ─▶ grava em [status_event]   (log de transições, append-only)
                          │
                          ├─ 0 → 1 : abre uma linha em [coffee_session] (started_at)
                          └─ 1 → 0 : fecha a coffee_session aberta (ended_at + duração)
   ▼
[Consultas da API]  alimentam US-02..US-07  ──▶  SignalR  ──▶  dashboard (UI5)
```

> **Robustez (US-01):** ao iniciar/reconectar, o serviço lê o último `status_event` do banco para saber o "valor anterior". Assim não conta uma transição falsa nem perde uma real após uma queda.

## Tabelas

### 1. `status_event` — log de transições (append-only)
Uma linha **só quando o status muda**. É a fonte da verdade e a trilha de auditoria.

| Coluna        | Tipo (sugestão)            | Descrição |
| ------------- | -------------------------- | --------- |
| `id`          | bigint, PK, identity       | Chave |
| `machine_code`| varchar(10)                | `"SAACE"` (já preparado para multimáquina) |
| `tag`         | int                        | `99` (Status H1) |
| `value`       | smallint                   | `0` ou `1` |
| `event_time`  | datetimeoffset / timestamptz | Horário do **payload** (com offset `-03:00`) |
| `received_at` | datetimeoffset / timestamptz | Quando o backend recebeu (para diagnóstico) |
| `raw_payload` | varchar(255)               | String original (auditoria/debug) |

Índice sugerido: `(machine_code, event_time)`.

### 2. `coffee_session` — sessões de uso (derivada)
Uma linha por ciclo **ligou → desligou**. Cada sessão = **1 acionamento** (US-07).

| Coluna            | Tipo (sugestão)            | Descrição |
| ----------------- | -------------------------- | --------- |
| `id`              | bigint, PK, identity       | Chave |
| `machine_code`    | varchar(10)                | `"SAACE"` |
| `started_at`      | datetimeoffset / timestamptz | Transição `0 → 1` |
| `ended_at`        | datetimeoffset / timestamptz, null | Transição `1 → 0` (null = em andamento) |
| `duration_seconds`| int, null                  | `ended_at - started_at` (preenchido ao fechar) |
| `is_open`         | bit / boolean              | `true` enquanto a sessão não fechou |

Índice sugerido: `(machine_code, started_at)`.

### 3. (Opcional) `hourly_usage` — agregado por hora
Pré-cálculo para acelerar gráficos. **Opcional** — na escala do hackathon dá para calcular tudo direto das duas tabelas acima. Só vale se a performance pedir.

| Coluna         | Tipo | Descrição |
| -------------- | ---- | --------- |
| `machine_code` | varchar(10) | `"SAACE"` |
| `day`          | date | Dia |
| `hour`         | smallint | 0–23 |
| `sessions_count` | int | Acionamentos na hora |
| `seconds_on`   | int | Tempo ligado acumulado na hora |

PK: `(machine_code, day, hour)`.

## Como cada história consulta o banco

| História | De onde vem | Lógica |
| -------- | ----------- | ------ |
| **US-02** Status atual | `status_event` | Último evento por `event_time` (ou status mantido em memória + push SignalR) |
| **US-07** Acionamentos do dia | `coffee_session` | `COUNT(*)` de sessões com `started_at` no dia de hoje |
| **US-03** Consumo por hora | `coffee_session` | Agrupar por hora de `started_at`: nº de sessões e/ou `SUM(duration_seconds)` |
| **US-04** Diário / semanal | `coffee_session` | Agregar por dia e por semana (sessões e tempo ligado) |
| **US-05** Horário de pico | `coffee_session` | Hora com **maior `SUM(duration_seconds)`** (maior tempo acumulado) |
| **US-06** Médias e tendência | `coffee_session` | Média diária de tempo/sessões; tendência = comparar janela recente vs. anterior |

## Decisões para você revisar (e mudar quando quiser)

1. **Motor de banco.** Sugestões:
   - **PostgreSQL** — grátis, ótimo para consultas analíticas, bom suporte .NET (Npgsql + EF Core).
   - **SQL Server** — familiar em ambiente .NET; Express/Developer são gratuitos.
   - **SQLite** — zero configuração, ótimo para começar rápido; menos indicado para concorrência/analytics num cenário real.
   - **SAP HANA** — alinhado ao stack SAP do desafio (OPEN Solutions é parceira SAP); banco relacional in-memory, orientado a colunas.
   > **Recomendação (estratégia de banco "destacável"):** desenvolver rápido em **PostgreSQL/SQLite** e **apontar para o SAP HANA no demo se houver acesso fácil** (idealmente uma instância de HANA Cloud fornecida pelo organizador). Se o acesso ao HANA for tranquilo, vale ir direto nele — pontua em aderência ao stack SAP.

   **Sobre o SAP HANA — viabilidade técnica:**
   - **Conexão .NET 10:** driver ADO.NET oficial (`Sap.Data.Hana`, NuGet `Sap.Data.Hana.Core.v2.1`). Encaixa bem com **Dapper** (SQL direto). **EF Core não tem provider oficial maduro** da SAP — com HANA, ir de **ADO.NET/Dapper** em vez de EF Core.
   - **Como subir a instância:** (a) **HANA Cloud trial** via SAP BTP — grátis, na nuvem, não pesa o PC, mas hiberna/expira; (b) **HANA Express (HXE)** — grátis, roda local (Docker/VM), porém **pesado** (~16GB RAM); (c) **instância do organizador** — caminho ideal, máxima aderência ao stack.
   - **Risco:** o desafio do HANA não é o conceito (é SQL como qualquer outro), é a **infra** (subir instância + provider EF frágil). Por isso manter o acesso a dados abstraído: trocar o banco no fim sai barato.
   - **Pendência:** confirmar com o time/organizador **se fornecem uma instância de SAP HANA Cloud**.

2. **Guardar só transições x guardar tudo.** A proposta guarda **só mudanças** (evita o "valor repetido"). Alternativa: gravar toda mensagem e marcar quais são transição — mais dados, mais auditoria, porém maior volume. Recomendo só transições.

3. **Sessão x calcular na hora.** A `coffee_session` deixa US-03..US-07 simples e rápidas. Alternativa: derivar tudo de `status_event` com window functions a cada consulta — menos tabelas, queries mais complexas. Recomendo manter a `coffee_session`.

4. **Fuso horário.** O payload já vem com o offset local (`-03:00`). Decisão: **converter no backend para o horário local** e persistir/exibir já condizente com o lugar — em vez de normalizar para UTC. Como a fonte já entrega o fuso correto, simplifica o tratamento e a exibição no dashboard.

5. **Multimáquina.** **Fora do escopo atual** — vamos monitorar só a SAACE. A coluna `machine_code` fica registrada apenas como **ideia de expansão futura** (se sobrar tempo), para não travar um crescimento posterior; não é objetivo agora.

6. **Estado "desligado de verdade".** Hoje só temos `0`/`1` com a máquina conectada. Quando soubermos o que ela envia desconectada (ou se para de publicar), dá para introduzir um terceiro estado (ex.: `offline`) sem mudar a estrutura — provavelmente uma flag/derivação por ausência de eventos.
   > **Hipótese a confirmar (ideia guardada, NÃO implementar ainda):** o `DATAINPUT` pode **parar de atualizar / ficar vazio** quando a máquina é realmente desligada. Se confirmado, ele serviria de base para detectar o estado `offline`. Aguardar coleta de dados da máquina desligada e o "pode fazer" antes de implementar.

## Resumo
- `status_event` = verdade (transições). `coffee_session` = uso (acionamentos + duração). `hourly_usage` = atalho opcional.
- Tudo que as US pedem sai dessas tabelas.
- Próximo passo (quando aprovar): transformar isso em entidades de domínio (DDD) + migration na solução .NET 10.
