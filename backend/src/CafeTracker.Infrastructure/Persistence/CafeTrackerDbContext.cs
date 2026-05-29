using CafeTracker.Application.Abstractions;
using CafeTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeTracker.Infrastructure.Persistence;

/// <summary>
/// Sessão com o banco (EF Core). Também cumpre o papel de <see cref="IUnitOfWork"/>,
/// já que o próprio DbContext rastreia as alterações e as confirma de uma vez.
/// </summary>
public sealed class CafeTrackerDbContext : DbContext, IUnitOfWork
{
    public CafeTrackerDbContext(DbContextOptions<CafeTrackerDbContext> options)
        : base(options)
    {
    }

    /// <summary>Tabela `status_event` — log de transições.</summary>
    public DbSet<StatusEvent> StatusEvents => Set<StatusEvent>();

    /// <summary>Tabela `coffee_session` — sessões de uso.</summary>
    public DbSet<CoffeeSession> CoffeeSessions => Set<CoffeeSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Aplica todas as classes *Configuration deste assembly automaticamente.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CafeTrackerDbContext).Assembly);
    }
}
