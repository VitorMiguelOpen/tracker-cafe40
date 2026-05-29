# Dados do sensor via MQTT (estrutura real)

> Telemetria de uma máquina industrial (CNC). O tema "café" é a roupagem do desafio; o dado real é de chão de fábrica (fuso, eixos, vibração, energia, contagem de peças).

## Broker

- **Host:** `opensolutionsbr.ddns.net`
- **QoS observado:** 0
- **Mensagens:** retidas (retained) — o broker entrega o último valor a quem assinar depois.

## Hierarquia de tópicos

```
IoT/
├── FABRICANTES        # mapa código → fabricante (ex.: Genesis, ISCAR, Mazak, RDM)
├── LINHAS             # mapa código → linha/máquina (ex.: "Máquina 4", "Tornos")
├── MODELOS            # mapa código → modelo (ex.: "INTEGREX i-300 (MTConnect)")
├── TABREFERENCIA      # JSON: dicionário de tags — GLOBAL, compartilhado por todas as máquinas
├── SAACE/             # uma máquina (código). SAACE = "IOT TRAINING ROOM"
│   ├── INFO           # IOT TRAINING ROOM;<id>;Padrão;<id>
│   ├── DADOS/         # subtópicos com valores de tags
│   ├── WIFI           # qualidade do sinal (ex.: -66dBm, 70%-241998829)
│   ├── SUBSCRIBE      # /IoT/<id>|<timestamp ISO>
│   ├── C1             # contador (ex.: 88)
│   ├── DATAINPUT      # ex.: FMM2|6
│   ├── H1             # STATUS ATUAL (tag 99) — valor cru (ex.: 0)
│   ├── DADOSPLUS      # tag|valor (ex.: 99|0)
│   ├── H1DATAHORA     # valor|epochSegundos (ex.: 0|1779983025)
│   ├── DADOSAPONTAMENTO  # evento completo (ver abaixo)
│   ├── SEGUNDOS       # ex.: 50333
│   └── SET            # ex.: CONPING|53115007
├── SAABK/  SAABT/  SAABU/  ...   # outras máquinas, mesma estrutura
```

> **`TABREFERENCIA` é global** (`/IoT/TABREFERENCIA`), não fica dentro de cada máquina. Para o escopo must have **não é necessário**: o `DADOSAPONTAMENTO` já traz o nome e a descrição da tag embutidos. Só seria útil para resolver o significado de *outras* tags.

## Formatos de payload

Os payloads são **strings delimitadas por `|`** (exceto `TABREFERENCIA`, que é JSON).

### DADOSAPONTAMENTO (principal para o nosso caso)

```
timestamp | tag | valor | nome | descrição |
```

Exemplo:
```
2026-05-28T15:43:45.000-03:00|99|0|Status (H1)|Machine status signal|
```

| Campo      | Exemplo                          | Observação                          |
| ---------- | -------------------------------- | ----------------------------------- |
| timestamp  | `2026-05-28T15:43:45.000-03:00`  | ISO 8601 com offset (-03:00)        |
| tag        | `99`                             | referência ao `TABREFERENCIA`       |
| valor      | `0`                              | valor da tag no momento             |
| nome       | `Status (H1)`                    | nome da tag                         |
| descrição  | `Machine status signal`          | descrição da tag                    |

### DADOSPLUS
```
tag|valor          → 99|0
```

### H1DATAHORA
```
valor|epochSegundos → 0|1779983025
```
`1779983025` em epoch (segundos) ≈ 28/05/2026.

## Status da máquina — H1 / tag 99 (CONFIRMADO)

O valor de H1 é `0` ou `1`. **Todos os dados coletados até agora são com a máquina energizada/conectada:**

| Valor | Significado |
| ----- | ----------- |
| `1`   | **Fazendo café** (ciclo ativo) — confirmado |
| `0`   | Parada / não está fazendo café (mas ligada/conectada) |

- **Pendente:** o estado **totalmente desligado/desconectado** ainda não foi observado. Não se sabe se a máquina para de publicar ou envia outro valor — dados a coletar.
- `H1`, `DADOSPLUS` (`99|X`) e `DADOSAPONTAMENTO` carregam o mesmo valor; o `X` do `DADOSPLUS` só assume `0` ou `1`.
- **Fonte canônica para persistir:** `DADOSAPONTAMENTO` — traz `timestamp ISO + tag + valor` em uma mensagem e publica a cada evento (confirmado via History).
- **O valor se repete:** o `DADOSAPONTAMENTO` publica de tempos em tempos, não só na mudança (visto `99|1` em mensagens consecutivas). Portanto, **contar acionamento exige detectar transição**: comparar com o valor anterior e contar 1 acionamento somente quando vai de `0 → 1` (US-07).

> **Atenção ao tópico:** o MQTT Explorer mostra o tópico de publish como `/IoT/SAACE/H1` (com **barra inicial**). A barra inicial faz parte do nome do tópico no MQTT — confirmar e usar exatamente igual no subscribe.

## Tags relevantes para o escopo "Café Tracker"

| Necessidade                         | Tag(s)                                  |
| ----------------------------------- | --------------------------------------- |
| Status ligado/desligado             | **99** `Status (H1)` + **100** `Status (Description)` |
| Modo de operação (auto/manual)      | **110**                                 |
| Acionamentos / peças no dia         | **800** `Part Count`, **97** `Total Pulse` |
| Consumo de energia (nice-to-have)   | **1020** energia (kW/h), **1030** corrente, **1010** frequência |
| Indicadores extras                  | **330** temp. fuso, **5001/5002/5003** vibração, **310/400-408** cargas, **98** horímetro |

## Pontos em aberto

- **Credenciais do broker:** usuário/senha e porta (1883 TCP ou 8883 TLS).

- **Estado "totalmente desligado":** coletar dados da máquina desconectada para saber o que ela publica (ou se para de publicar).

> **Resolvido:** `1` = fazendo café (confirmado), escopo (somente máquina **SAACE**) e fonte canônica de eventos (**DADOSAPONTAMENTO**, com detecção de transição para acionamentos).
