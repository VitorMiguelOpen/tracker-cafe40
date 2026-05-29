namespace CafeTracker.Application.Queries;

/// <summary>
/// Porta de leitura das consultas analíticas de consumo (US-03 a US-06 + métrica
/// de acionamentos). As datas são interpretadas no fuso local configurado da
/// aplicação (decisão de fuso em docs/banco.md).
/// </summary>
public interface IConsumptionQueries
{
    /// <summary>US-03: 24 faixas horárias (0..23) do dia informado, com zeros preenchidos.</summary>
    Task<IReadOnlyList<HourlyConsumptionDto>> GetHourlyAsync(
        string machineCode, DateOnly date, CancellationToken ct = default);

    /// <summary>US-04: consolidado de um único dia.</summary>
    Task<DailyConsumptionDto> GetDailyAsync(
        string machineCode, DateOnly date, CancellationToken ct = default);

    /// <summary>US-04: 7 dias (segunda a domingo) da semana que contém a data informada.</summary>
    Task<IReadOnlyList<DailyConsumptionDto>> GetWeeklyAsync(
        string machineCode, DateOnly anyDateInWeek, CancellationToken ct = default);

    /// <summary>US-05: hora com maior tempo acumulado de uso no dia informado.</summary>
    Task<PeakHourDto> GetPeakHourAsync(
        string machineCode, DateOnly date, CancellationToken ct = default);

    /// <summary>US-06: média diária e tendência nos últimos <paramref name="days"/> dias.</summary>
    Task<TrendDto> GetTrendAsync(
        string machineCode, int days, CancellationToken ct = default);

    /// <summary>Métrica derivada: total de acionamentos de hoje.</summary>
    Task<ActivationsTodayDto> GetActivationsTodayAsync(
        string machineCode, CancellationToken ct = default);
}
