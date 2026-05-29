using CafeTracker.Application.Abstractions;
using CafeTracker.Application.Queries;
using CafeTracker.Infrastructure.Messaging;
using CafeTracker.Infrastructure.Persistence;
using CafeTracker.Infrastructure.Persistence.Queries;
using CafeTracker.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CafeTracker.Infrastructure;

/// <summary>Registro de DI da camada Infrastructure (banco + MQTT).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1) Banco: EF Core + PostgreSQL.
        var connectionString = configuration.GetConnectionString("Default")
            ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        services.AddDbContext<CafeTrackerDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 2) Repositórios e Unit of Work (o DbContext É a unidade de trabalho).
        services.AddScoped<IStatusEventRepository, StatusEventRepository>();
        services.AddScoped<ICoffeeSessionRepository, CoffeeSessionRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CafeTrackerDbContext>());

        // 2b) Consultas de leitura (lado de query) usadas pela Api/dashboard.
        services.Configure<AppQuerySettings>(configuration.GetSection(AppQuerySettings.SectionName));
        services.AddScoped<IStatusQueries, StatusQueries>();
        services.AddScoped<IConsumptionQueries, ConsumptionQueries>();

        // 3) Configuração tipada do MQTT (lê a seção "Mqtt").
        services.Configure<MqttSettings>(configuration.GetSection(MqttSettings.SectionName));

        // 4) Serviço de fundo que conecta no broker e ingere as leituras.
        services.AddHostedService<MqttIngestionService>();

        return services;
    }
}
