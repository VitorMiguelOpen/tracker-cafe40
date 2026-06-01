using CafeTracker.Api.Configuration;
using CafeTracker.Api.Realtime;
using CafeTracker.Application;
using CafeTracker.Application.Abstractions;
using CafeTracker.Infrastructure;
using CafeTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// 1) Carrega o .env da raiz ANTES de tudo (segredos: usuário/senha MQTT, banco, URLs).
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// 2) Mapeia variáveis sensíveis do .env (nomes MQTT_*) para a configuração tipada
//    (seção "Mqtt") e a connection string. Só sobrescreve quando o valor existe,
//    para não apagar os padrões do appsettings.
var envOverrides = new Dictionary<string, string?>();
AddIfPresent(envOverrides, "Mqtt:Username", "MQTT_USERNAME");
AddIfPresent(envOverrides, "Mqtt:Password", "MQTT_PASSWORD");
AddIfPresent(envOverrides, "Mqtt:Host", "MQTT_HOST");
AddIfPresent(envOverrides, "Mqtt:Topic", "MQTT_TOPIC");
AddIfPresent(envOverrides, "ConnectionStrings:Default", "DB_CONNECTION_STRING");
if (envOverrides.Count > 0)
    builder.Configuration.AddInMemoryCollection(envOverrides);

// 3) Camadas da aplicação (DDD).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 4) Tempo real (SignalR) + a implementação concreta da porta de notificação.
//    É isto que faltava para a ingestão MQTT conseguir "empurrar" o status.
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();

// 5) CORS para o dashboard UI5 (origens vêm do appsettings → "Cors:Origins").
const string CorsPolicy = "frontend";
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
    policy.WithOrigins(corsOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials())); // necessário para o SignalR com origem específica

// 6) API + documentação (OpenAPI).
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 7) Garante o schema do banco no startup.
//    - PostgreSQL (produção): aplica as migrações versionadas.
//    - SQLite (dev): cria o schema a partir do modelo (sem migrações, que são
//      específicas do Npgsql), permitindo rodar sem Docker/Postgres.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CafeTrackerDbContext>();
    if (db.Database.IsNpgsql())
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();
}

// 8) Pipeline HTTP.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors(CorsPolicy);
app.MapControllers();
app.MapHub<StatusHub>("/hubs/status"); // dashboard conecta aqui

app.Run();

// --- Função local: adiciona o par chave→valor só se a variável existir e não for vazia.
static void AddIfPresent(IDictionary<string, string?> target, string configKey, string envName)
{
    var value = Environment.GetEnvironmentVariable(envName);
    if (!string.IsNullOrWhiteSpace(value))
        target[configKey] = value;
}
