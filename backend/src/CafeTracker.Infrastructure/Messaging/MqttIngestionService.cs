using CafeTracker.Application.Ingestion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace CafeTracker.Infrastructure.Messaging;

/// <summary>
/// Serviço de fundo que mantém a conexão com o broker MQTT viva, assina o tópico
/// DADOSAPONTAMENTO e, a cada mensagem, faz o parse e chama o
/// <see cref="StatusIngestionService"/>. Reconecta automaticamente se cair.
/// (Esqueleto inspirado no projeto de referência Open.Drive, corrigido.)
/// </summary>
public sealed class MqttIngestionService : BackgroundService
{
    private readonly MqttSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttIngestionService> _logger;

    private IMqttClient? _client;
    private MqttClientOptions? _options;

    public MqttIngestionService(
        IOptions<MqttSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<MqttIngestionService> logger)
    {
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new MqttFactory().CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithClientId(_settings.ClientId)
            .WithTcpServer(_settings.Host, _settings.Port)
            .WithCredentials(_settings.Username, _settings.Password)
            .WithKeepAlivePeriod(TimeSpan.FromMinutes(1))
            .WithCleanSession()
            .Build();

        // Cada mensagem recebida cai aqui.
        _client.ApplicationMessageReceivedAsync += HandleMessageAsync;
        // Só loga; a reconexão de fato é feita pelo loop guardião abaixo.
        _client.DisconnectedAsync += e =>
        {
            _logger.LogWarning("MQTT desconectado: {Reason}", e.Reason);
            return Task.CompletedTask;
        };

        await ConnectAndSubscribeAsync(stoppingToken);

        // Loop guardião: a cada N segundos confere a conexão e reassina se caiu.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_settings.ReconnectDelaySeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // shutdown normal
            }

            if (_client is { IsConnected: false })
            {
                _logger.LogWarning("Conexão MQTT ausente — tentando reconectar...");
                await ConnectAndSubscribeAsync(stoppingToken);
            }
        }
    }

    private async Task ConnectAndSubscribeAsync(CancellationToken ct)
    {
        try
        {
            if (_client is null || _client.IsConnected)
                return;

            await _client.ConnectAsync(_options!, ct);

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f => f
                    .WithTopic(_settings.Topic)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)) // QoS 0
                .Build();

            await _client.SubscribeAsync(subscribeOptions, ct);

            _logger.LogInformation(
                "MQTT conectado em {Host}:{Port}, assinando {Topic}.",
                _settings.Host, _settings.Port, _settings.Topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao conectar/assinar no broker MQTT.");
        }
    }

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var payload = e.ApplicationMessage.ConvertPayloadToString();

        if (!DadosApontamentoParser.TryParse(payload, _settings.MachineCode, out var reading)
            || reading is null)
        {
            _logger.LogWarning("Payload MQTT em formato inesperado, ignorado: {Payload}", payload);
            return;
        }

        try
        {
            // BackgroundService é singleton; o DbContext é scoped → criamos um escopo por mensagem.
            using var scope = _scopeFactory.CreateScope();
            var ingestion = scope.ServiceProvider.GetRequiredService<StatusIngestionService>();
            await ingestion.IngestAsync(reading);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar a leitura MQTT.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client is { IsConnected: true })
            await _client.DisconnectAsync();

        await base.StopAsync(cancellationToken);
    }
}
