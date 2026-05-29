using CafeTracker.Application.Queries;
using CafeTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CafeTracker.Infrastructure.Persistence.Queries;

/// <summary>
/// Implementação (EF Core) da consulta de status atual (US-02).
/// Somente leitura — usa <c>AsNoTracking</c>.
/// </summary>
public sealed class StatusQueries : IStatusQueries
{
    private readonly CafeTrackerDbContext _db;

    public StatusQueries(CafeTrackerDbContext db) => _db = db;

    public async Task<CurrentStatusDto?> GetCurrentAsync(string machineCode, CancellationToken ct = default)
    {
        // O último evento (transição) define o status atual.
        var last = await _db.StatusEvents
            .AsNoTracking()
            .Where(e => e.MachineCode == machineCode)
            .OrderByDescending(e => e.EventTime)
            .ThenByDescending(e => e.Id)
            .FirstOrDefaultAsync(ct);

        if (last is null)
            return null;

        return new CurrentStatusDto(
            last.MachineCode,
            last.Value,
            ((MachineState)last.Value).ToString(),
            last.Value == (short)MachineState.Brewing ? "Ligado" : "Desligado",
            last.EventTime);
    }
}
