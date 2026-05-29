using CafeTracker.Domain.Enums;

namespace CafeTracker.Domain.Entities;

/// <summary>
/// Log de TRANSIÇÃO de status (append-only). Uma linha só é criada quando o
/// valor do status realmente MUDA — é a fonte da verdade e a trilha de auditoria.
/// Corresponde à tabela `status_event` descrita em docs/banco.md.
/// </summary>
public sealed class StatusEvent
{
    /// <summary>Chave primária (gerada pelo banco).</summary>
    public long Id { get; private set; }

    /// <summary>Código da máquina de origem (ex.: "SAACE"). Preparado para multimáquina.</summary>
    public string MachineCode { get; private set; } = null!;

    /// <summary>Tag do dado no MQTT (99 = Status H1).</summary>
    public int Tag { get; private set; }

    /// <summary>Valor cru do status: 0 (parada) ou 1 (fazendo café).</summary>
    public short Value { get; private set; }

    /// <summary>Horário informado no payload do MQTT (já com o fuso de origem).</summary>
    public DateTimeOffset EventTime { get; private set; }

    /// <summary>Horário em que o backend recebeu a mensagem (diagnóstico).</summary>
    public DateTimeOffset ReceivedAt { get; private set; }

    /// <summary>String original recebida no MQTT (auditoria/debug).</summary>
    public string? RawPayload { get; private set; }

    /// <summary>Interpretação do valor cru como estado de domínio.</summary>
    public MachineState State => (MachineState)Value;

    // Construtor sem parâmetros exigido pelo EF Core para materializar do banco.
    private StatusEvent() { }

    /// <summary>Cria um novo evento de transição já validado.</summary>
    public StatusEvent(
        string machineCode,
        int tag,
        short value,
        DateTimeOffset eventTime,
        DateTimeOffset receivedAt,
        string? rawPayload)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
            throw new ArgumentException("MachineCode é obrigatório.", nameof(machineCode));

        MachineCode = machineCode;
        Tag = tag;
        Value = value;
        EventTime = eventTime;
        ReceivedAt = receivedAt;
        RawPayload = rawPayload;
    }
}
