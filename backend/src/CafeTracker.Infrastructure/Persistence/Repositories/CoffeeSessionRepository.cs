using CafeTracker.Application.Abstractions;
using CafeTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeTracker.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core de <see cref="ICoffeeSessionRepository"/>.</summary>
public sealed class CoffeeSessionRepository : ICoffeeSessionRepository
{
    private readonly CafeTrackerDbContext _db;

    public CoffeeSessionRepository(CafeTrackerDbContext db) => _db = db;

    public Task<CoffeeSession?> GetOpenAsync(string machineCode, CancellationToken ct = default) =>
        _db.CoffeeSessions
            .Where(s => s.MachineCode == machineCode && s.IsOpen)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(CoffeeSession session, CancellationToken ct = default) =>
        await _db.CoffeeSessions.AddAsync(session, ct);

    public void Update(CoffeeSession session) =>
        _db.CoffeeSessions.Update(session);
}
