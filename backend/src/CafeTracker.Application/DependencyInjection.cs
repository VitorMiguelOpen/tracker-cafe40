using CafeTracker.Application.Ingestion;
using Microsoft.Extensions.DependencyInjection;

namespace CafeTracker.Application;

/// <summary>Registro de DI da camada Application.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped: depende dos repositórios/DbContext (também scoped).
        services.AddScoped<StatusIngestionService>();
        return services;
    }
}
