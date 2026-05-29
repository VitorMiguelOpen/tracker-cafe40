using CafeTracker.Application.Realtime;

namespace CafeTracker.Application.Abstractions;

/// <summary>
/// Porta de notificação em tempo real. A implementação concreta (SignalR)
/// vive na Infrastructure/Api — a Application só conhece este contrato.
/// </summary>
public interface IRealtimeNotifier
{
    Task NotifyStatusChangedAsync(StatusChangedNotification notification, CancellationToken ct = default);
}
