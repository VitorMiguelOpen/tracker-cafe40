using CafeTracker.Domain.Enums;

namespace CafeTracker.Application.Realtime;

/// <summary>
/// Mensagem enviada ao dashboard em tempo real quando o status MUDA.
/// (Transportada via SignalR — mas a Application não conhece o SignalR,
/// só este contrato.)
/// </summary>
public sealed record StatusChangedNotification(
    string MachineCode,
    short Value,
    MachineState State,
    DateTimeOffset EventTime);
