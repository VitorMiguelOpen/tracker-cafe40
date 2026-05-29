namespace CafeTracker.Domain.Enums;

/// <summary>
/// Estado da máquina conforme o status H1 (tag 99) publicado no MQTT.
/// O valor cru é 0 ou 1 — confirmado na análise dos dados (docs/dados-mqtt.md).
/// </summary>
public enum MachineState : short
{
    /// <summary>0 = máquina ligada/conectada, porém sem fazer café (parada).</summary>
    Stopped = 0,

    /// <summary>1 = ciclo ativo: a máquina está fazendo café (CONFIRMADO).</summary>
    Brewing = 1
}
