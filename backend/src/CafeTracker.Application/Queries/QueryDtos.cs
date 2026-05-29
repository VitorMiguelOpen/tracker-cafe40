namespace CafeTracker.Application.Queries;

/// <summary>
/// Estado atual do equipamento (US-02). <paramref name="State"/> é o nome do
/// estado de domínio (Brewing/Stopped) e <paramref name="Label"/> é o texto
/// pronto para o dashboard ("Ligado"/"Desligado").
/// </summary>
public sealed record CurrentStatusDto(
    string MachineCode,
    short Value,
    string State,
    string Label,
    DateTimeOffset? EventTime);

/// <summary>
/// Uma faixa de uma hora (0..23) do gráfico de consumo por hora (US-03).
/// <paramref name="Activations"/> = nº de acionamentos iniciados nessa hora;
/// <paramref name="TotalSeconds"/> = tempo acumulado de uso (insumo da US-05).
/// </summary>
public sealed record HourlyConsumptionDto(int Hour, int Activations, long TotalSeconds);

/// <summary>Consolidado de um dia (US-04): acionamentos e tempo total de uso.</summary>
public sealed record DailyConsumptionDto(DateOnly Date, int Activations, long TotalSeconds);

/// <summary>
/// Horário de pico (US-05): a hora com MAIOR tempo acumulado de uso.
/// <paramref name="Hour"/> é null quando não há dados no período.
/// </summary>
public sealed record PeakHourDto(int? Hour, long TotalSeconds);

/// <summary>
/// Média diária e tendência (US-06). <paramref name="Trend"/> assume um dos
/// três estados oficiais: "Aumentando", "Estável" ou "Diminuindo".
/// </summary>
public sealed record TrendDto(
    double AverageSecondsPerDay,
    string Trend,
    IReadOnlyList<DailyConsumptionDto> Days);

/// <summary>Total de acionamentos do dia (métrica derivada da US-03).</summary>
public sealed record ActivationsTodayDto(DateOnly Date, int Activations);
