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
│   ├── SET            # ex.: CONPING|53115007
│   └── TABREFERENCIA  # JSON: dicionário de todas as tags
├── SAABK/  SAABT/  SAABU/  ...   # outras máquinas, mesma estrutura
```

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

## Tags relevantes para o escopo "Café Tracker"

| Necessidade                         | Tag(s)                                  |
| ----------------------------------- | --------------------------------------- |
| Status ligado/desligado             | **99** `Status (H1)` + **100** `Status (Description)` |
| Modo de operação (auto/manual)      | **110**                                 |
| Acionamentos / peças no dia         | **800** `Part Count`, **97** `Total Pulse` |
| Consumo de energia (nice-to-have)   | **1020** energia (kW/h), **1030** corrente, **1010** frequência |
| Indicadores extras                  | **330** temp. fuso, **5001/5002/5003** vibração, **310/400-408** cargas, **98** horímetro |

## Pontos em aberto (a confirmar com o cliente / observando o broker)

- **Significado dos códigos de status (tag 99 / H1):** vimos `0`. Falta o mapa do que cada valor representa (ex.: 0 = parado, 1 = em execução, ...). A tag **100** (`Status (Description)`) deve trazer o texto correspondente.
- **Vamos monitorar uma máquina (SAACE) ou todas?** A estrutura suporta múltiplas.
- **Frequência de publicação** e se cada mudança de status gera um novo `DADOSAPONTAMENTO`.
