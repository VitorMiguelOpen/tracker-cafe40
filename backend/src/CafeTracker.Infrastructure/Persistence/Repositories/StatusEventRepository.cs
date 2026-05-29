using CafeTracker.Application.Abstractions;
using CafeTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeTracker.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core de <see cref="IStatusEventRepository"/>.</summary>
public sealed class StatusEventRepository : IStatusEventRepository
{
    private readonly CafeTrackerDbContext _db;

    public StatusEventRepository(CafeTrackerDbContext db) => _db = db;

    public Task<StatusEvent?> GetLastAsync(string machineCode, CancellationToken ct = default) =>
        _db.StatusEvents
            .Where(e => e.MachineCode == machineCode)
            .OrderByDescending(e => e.EventTime)
            .ThenByDescending(e => e.Id)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(StatusEvent statusEvent, CancellationToken ct = default) =>
        await _db.StatusEvents.AddAsync(statusEvent, ct);
}
