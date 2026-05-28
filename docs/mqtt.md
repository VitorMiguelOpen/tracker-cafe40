# MQTT — conceitos e conexão

Resumo de apoio para o time (base: apresentação interna de 22/05/2026).

## O que é

MQTT (Message Queuing Telemetry Transport) é um protocolo **leve** de mensageria no modelo **publish/subscribe**, criado em 1999 (IBM), padrão aberto (OASIS/ISO), rodando sobre TCP/IP. É o protocolo mais usado em IoT e telemetria.

## Por que usar

Leve (pouca banda/CPU), desacoplado (produtor e consumidor não se conhecem), escalável, confiável (QoS, sessões, reconexão), tempo real (push) e simples (`connect`, `publish`, `subscribe`).

## Conceitos-chave

| Termo        | Significado                                            |
| ------------ | ------------------------------------------------------ |
| Broker       | Servidor central que recebe e distribui mensagens      |
| Client       | Dispositivo/app conectado (sensor, painel, sistema)    |
| Publisher    | Client que publica mensagem em um tópico               |
| Subscriber   | Client que assina um tópico para receber mensagens     |
| Tópico       | Endereço/canal hierárquico (separado por `/`)          |
| Mensagem     | Payload enviado (texto, JSON, binário)                 |

## Tópicos

Hierarquia separada por `/`. Coringas:
- `+` — um nível: `fabrica/linha1/+/temperatura`
- `#` — vários níveis: `fabrica/linha1/#`

## QoS (qualidade de serviço)

- **QoS 0** — no máximo uma vez ("dispara e esquece"); pode perder.
- **QoS 1** — pelo menos uma vez; pode duplicar (equilíbrio mais comum).
- **QoS 2** — exatamente uma vez; mais lento (handshake de 4 etapas).

## Recursos importantes

- **Retained message** — broker guarda a última mensagem do tópico e entrega a quem assinar depois (útil para o status atual).
- **Last Will (LWT)** — aviso automático quando um client cai sem avisar.
- **Sessão & Keep-Alive** — reconexão automática e entrega de pendências.
- **Segurança** — TLS (porta 8883), usuário/senha e ACLs.

## Como conectar (passo a passo)

1. Endereço + porta do broker (1883 TCP, 8883 TLS, 8083/8084 WebSocket).
2. Client ID único.
3. Autenticação (usuário/senha e/ou TLS).
4. `Connect → CONNACK`.
5. `Subscribe` / `Publish` nos tópicos.
6. Keep-alive + reconexão para manter a conexão viva.

## Exemplo de fluxo (genérico)

```text
broker   = "broker.exemplo.com"
porta    = 8883          # TLS
clientId = "sensor-001"

connect(broker, porta, clientId, tls=True)
subscribe("fabrica/linha1/cafeteira/status", qos=1)
publish("fabrica/linha1/cafeteira/status", "online", qos=1, retain=True)
on_message(topico, payload): processar(topico, payload)
```

## Ferramentas para testar

- **Brokers:** Mosquitto (local/edge), EMQX, HiveMQ, ou nuvem (AWS IoT Core, Azure IoT Hub, HiveMQ Cloud).
- **Clientes de teste:** MQTT Explorer (GUI), MQTTX (desktop/CLI), `mosquitto_pub`/`mosquitto_sub`.
