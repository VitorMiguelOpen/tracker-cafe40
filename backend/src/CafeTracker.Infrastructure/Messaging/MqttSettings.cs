namespace CafeTracker.Infrastructure.Messaging;

/// <summary>
/// Configuração do broker MQTT (lida da seção "Mqtt" do appsettings;
/// credenciais sensíveis vêm do .env / variáveis de ambiente).
/// </summary>
public sealed class MqttSettings
{
    public const string SectionName = "Mqtt";

    /// <summary>Host do broker (ex.: opensolutionsbr.ddns.net).</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Porta TCP (1883 sem TLS).</summary>
    public int Port { get; set; } = 1883;

    /// <summary>Identificador do cliente nesta conexão.</summary>
    public string ClientId { get; set; } = "cafe-tracker-backend";

    /// <summary>Tópico assinado (com a barra inicial, como o broker exige).</summary>
    public string Topic { get; set; } = "/IoT/SAACE/DADOSAPONTAMENTO";

    /// <summary>Código da máquina monitorada (SAACE).</summary>
    public string MachineCode { get; set; } = "SAACE";

    /// <summary>Usuário (vem do .env — não versionado).</summary>
    public string? Username { get; set; }

    /// <summary>Senha (vem do .env — não versionado).</summary>
    public string? Password { get; set; }

    /// <summary>Segundos de espera antes de tentar reconectar.</summary>
    public int ReconnectDelaySeconds { get; set; } = 5;
}
