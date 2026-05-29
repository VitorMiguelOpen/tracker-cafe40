using CafeTracker.Application.Queries;
using CafeTracker.Infrastructure.Persistence.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CafeTracker.Api.Controllers;

/// <summary>
/// Endpoints de status atual do equipamento (US-02). O tempo real em si chega
/// pelo SignalR; este endpoint serve para o dashboard pegar o estado inicial
/// ao abrir a tela.
/// </summary>
[ApiController]
[Route("api/status")]
public sealed class StatusController : ControllerBase
{
    private readonly IStatusQueries _status;
    private readonly AppQuerySettings _settings;

    public StatusController(IStatusQueries status, IOptions<AppQuerySettings> settings)
    {
        _status = status;
        _settings = settings.Value;
    }

    /// <summary>Status atual da máquina (último evento conhecido).</summary>
    [HttpGet("current")]
    public async Task<ActionResult<CurrentStatusDto>> GetCurrent(
        [FromQuery] string? machine, CancellationToken ct)
    {
        var machineCode = string.IsNullOrWhiteSpace(machine) ? _settings.MachineCode : machine;
        var current = await _status.GetCurrentAsync(machineCode, ct);

        // Sem eventos ainda → assume "Desligado" (o primeiro evento atualiza).
        return Ok(current ?? new CurrentStatusDto(machineCode, 0, "Stopped", "Desligado", null));
    }
}
