using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CafeTracker.Infrastructure.Persistence;

/// <summary>
/// Usado apenas em tempo de DESIGN (ex.: `dotnet ef migrations add`).
/// Permite ao EF construir o DbContext sem subir a API. A connection string
/// aqui é só um placeholder — gerar migration NÃO acessa o banco; só
/// `dotnet ef database update` precisaria de um banco real.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CafeTrackerDbContext>
{
    public CafeTrackerDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=cafetracker;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<CafeTrackerDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CafeTrackerDbContext(options);
    }
}
