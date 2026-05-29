namespace CafeTracker.Infrastructure.Persistence.Queries;

/// <summary>
/// Configurações da aplicação que afetam as consultas (seção "App" do appsettings).
/// </summary>
public sealed class AppQuerySettings
{
    public const string SectionName = "App";

    /// <summary>Código da máquina monitorada (padrão das consultas). Ex.: "SAACE".</summary>
    public string MachineCode { get; set; } = "SAACE";

    /// <summary>
    /// Deslocamento de fuso, em horas, usado para agrupar eventos por hora/dia no
    /// horário local (decisão de fuso em docs/banco.md). Brasil = -3.
    /// Usamos um offset fixo (em vez de TimeZoneInfo) para ser determinístico e
    /// independente do SO (Windows × Linux usam IDs de fuso diferentes).
    /// </summary>
    public int TimeZoneOffsetHours { get; set; } = -3;
}
