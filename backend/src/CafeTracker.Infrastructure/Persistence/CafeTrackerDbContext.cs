using CafeTracker.Application.Abstractions;
using CafeTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // O provider SQLite (usado em desenvolvimento) não traduz ORDER BY sobre
        // DateTimeOffset. Convertemos esses valores para um formato binário ordenável
        // (preservando o instante), o que permite ordenar no banco.
        if (Database.IsSqlite())
        {
            configurationBuilder
                .Properties<DateTimeOffset>()
                .HaveConversion<DateTimeOffsetToBinaryConverter>();
        }
        // No PostgreSQL o tipo nativo é timestamptz, que pelo Npgsql só aceita
        // DateTimeOffset em UTC (offset 0). Os dados chegam com offset local (ex.: -03:00),
        // então normalizamos para UTC ao gravar — o que também é aplicado aos parâmetros
        // das consultas por intervalo. Isso casa com a estratégia das queries, que guardam
        // o instante em UTC e reconstroem o horário local em memória (ToLocal). Como
        // DateTimeOffset é um instante absoluto, a conversão não altera nenhum resultado.
        else if (Database.IsNpgsql())
        {
            configurationBuilder
                .Properties<DateTimeOffset>()
                .HaveConversion<UtcDateTimeOffsetConverter>();
        }
    }

    /// <summary>
    /// Converte <see cref="DateTimeOffset"/> para UTC na escrita (o PostgreSQL/Npgsql
    /// exige offset 0 em timestamptz). Na leitura mantém o valor (já vem em UTC).
    /// </summary>
    private sealed class UtcDateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
    {
        public UtcDateTimeOffsetConverter()
            : base(v => v.ToUniversalTime(), v => v)
        {
        }
    }
}
