using CafeTracker.Application.Abstractions;
using CafeTracker.Application.Realtime;
using CafeTracker.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace CafeTracker.Api.Realtime;

/// <summary>
/// Implementação concreta da porta <see cref="IRealtimeNotifier"/> usando SignalR.
/// É aqui que a notificação de mudança de status (vinda da ingestão MQTT) é
/// empurrada para todos os dashboards conectados — atendendo o "tempo real" da US-02.
/// </summary>
public sealed class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<StatusHub> _hub;

    public SignalRNotifier(IHubContext<StatusHub> hub) => _hub = hub;

    public Task NotifyStatusChangedAsync(StatusChangedNotification notification, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync(
            "StatusChanged",
            new
            {
                machineCode = notification.MachineCode,
                value = notification.Value,
                state = notification.State.ToString(),
                label = notification.Value == (short)MachineState.Brewing ? "Ligado" : "Desligado",
                eventTime = notification.EventTime
            },
            ct);
}
