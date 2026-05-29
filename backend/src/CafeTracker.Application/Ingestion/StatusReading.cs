namespace CafeTracker.Application.Ingestion;

/// <summary>
/// Uma leitura de status já interpretada a partir do payload do MQTT
/// (DADOSAPONTAMENTO: "timestamp|tag|valor|nome|descrição").
/// O parsing da string crua acontece na Infrastructure; aqui o dado já chega limpo.
/// </summary>
public sealed record StatusReading(
    string MachineCode,
    int Tag,
    short Value,
    DateTimeOffset EventTime,
    string? RawPayload);
