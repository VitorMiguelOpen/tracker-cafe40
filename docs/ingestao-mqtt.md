# Plano de ingestão MQTT (backend)

Como o backend .NET 10 vai conectar no broker, receber os eventos da máquina **SAACE** e transformá-los em dados para o dashboard.

> Baseado na análise do projeto de referência **Open.Drive** (Windows Service em .NET 8 + MQTTnet 4.3), reaproveitando o esqueleto de conexão e descartando o que não serve (SMB, file watcher, Topshelf).

## Fluxo alvo

```
BackgroundService
  → conecta no broker  opensolutionsbr.ddns.net
  → subscribe em  IoT/SAACE/DADOSAPONTAMENTO  (QoS 0)
  → ApplicationMessageReceivedAsync: lê o payload (UTF-8)
  → parse  "timestamp|tag|valor|nome|descrição"
  → cria evento de domínio  →  persiste no banco
  → empurra atualização via SignalR  →  dashboard UI5
  → DisconnectedAsync: reconecta
```

## Tecnologias

- **MQTTnet** — biblioteca cliente MQTT. O projeto de referência usa a **v4.3.x**; para reaproveitar o código 1:1, fixar a v4.3.x (a v5 mudou a API: `MqttClientFactory`, `Payload` virou `ReadOnlySequence`).
- **Generic Host** (`Microsoft.Extensions.Hosting`) + **DI** + `IOptions<MqttSettings>`.
- **BackgroundService** (`IHostedService`) para manter a conexão viva.

## Configuração (appsettings / Connections.json)

```json
{
  "MqttSettings": {
    "Broker": "opensolutionsbr.ddns.net",
    "Port": 1883,
    "ClientId": "cafe-tracker-backend",
    "Topic": "IoT/SAACE",
    "Username": "",
    "Password": ""
  }
}
```

> Credenciais reais (usuário/senha/porta) ficam **fora do versionamento** (.env / user-secrets). O `.env.example` na raiz tem o modelo.

## Conexão (reaproveitado do Open.Drive)

```csharp
var options = new MqttClientOptionsBuilder()
    .WithClientId(settings.ClientId)
    .WithTcpServer(settings.Broker, settings.Port)
    .WithCredentials(settings.Username, settings.Password)
    .WithKeepAlivePeriod(TimeSpan.FromMinutes(1))
    .WithCleanSession()
    .Build();

await mqttClient.ConnectAsync(options);
```

## Assinatura

```csharp
await mqttClient.SubscribeAsync(
    new MqttTopicFilterBuilder()
        .WithTopic($"{settings.Topic}/DADOSAPONTAMENTO")   // IoT/SAACE/DADOSAPONTAMENTO
        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)  // QoS 0, casa com o broker
        .Build());
```

## Recebimento + parse do payload

O payload **não é JSON** — é uma string delimitada por `|`:

```
2026-05-28T15:43:45.000-03:00|99|0|Status (H1)|Machine status signal|
        timestamp            |tag|val|  nome    |     descrição      |
```

```csharp
mqttClient.ApplicationMessageReceivedAsync += async e =>
{
    var raw = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
    var parts = raw.Split('|');
    // parts[0] = timestamp (ISO 8601 com offset)
    // parts[1] = tag        (ex.: "99")
    // parts[2] = valor      (ex.: "0")
    // parts[3] = nome       (ex.: "Status (H1)")
    // parts[4] = descrição
    // → montar evento de domínio, persistir e notificar (SignalR)
};
```

## Reconexão

Dois mecanismos (como no Open.Drive, porém corrigidos):

1. **Evento** `DisconnectedAsync += reconectar` (com backoff).
2. **Loop** no `BackgroundService`: a cada poucos segundos verifica `IsConnected` e re-assina se caiu.

## Diferenças em relação ao Open.Drive (o que NÃO copiar)

| Open.Drive faz | Café Tracker faz |
| -------------- | ---------------- |
| Desserializa JSON (`MessageObject`) | Parse de string por `Split('\|')` |
| QoS 2 (ExactlyOnce) | QoS 0 (casa com o broker da máquina) |
| Move arquivos via SMB + `FileSystemWatcher` | Persiste evento no banco + push SignalR |
| Roda como Windows Service (Topshelf) | API .NET 10 (Web Host) |
| Dois `IMqttClient` (singleton + interno) → **bug** | **Um único** `IMqttClient` |
| `ConnectAsync()`/`SubscribeAsync()` sem `await` → **bug** | Sempre `await` |

## Status confirmado

H1 / tag 99: `1` = **fazendo café** (confirmado), `0` = parada/não fazendo café (máquina ainda ligada). Estado totalmente desligado ainda não observado. Detalhes em [dados-mqtt.md](dados-mqtt.md).

## Fonte canônica de eventos (CONFIRMADO)

`DADOSAPONTAMENTO` (`/IoT/SAACE/DADOSAPONTAMENTO`) — publica a cada evento com `timestamp ISO + tag + valor`. **O valor se repete** (publica periodicamente, não só na mudança), então o serviço deve guardar o **último valor** e contar acionamento só na transição `0 → 1` (US-07).

## Pendências para fechar a implementação

- **Credenciais reais do broker** (usuário, senha, porta) — confirmar se exige autenticação e se é 1883 (TCP) ou TLS (8883).
- Atenção à **barra inicial** do tópico (`/IoT/SAACE/...`) no subscribe.
