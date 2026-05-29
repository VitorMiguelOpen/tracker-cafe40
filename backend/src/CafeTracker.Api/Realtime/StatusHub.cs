using Microsoft.AspNetCore.SignalR;

namespace CafeTracker.Api.Realtime;

/// <summary>
/// Hub SignalR do dashboard. O dashboard (UI5) se conecta em <c>/hubs/status</c>
/// e recebe o evento "StatusChanged" sempre que o status do equipamento muda.
/// Não há métodos de cliente→servidor: o fluxo é só servidor→cliente (push).
/// </summary>
public sealed class StatusHub : Hub
{
}
