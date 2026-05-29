using CafeTracker.Application.Queries;
using CafeTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CafeTracker.Infrastructure.Persistence.Queries;

/// <summary>
/// Implementação (EF Core) das consultas analíticas de consumo (US-03 a US-06).
///
/// Estratégia: o banco guarda os instantes em UTC (timestamptz). Buscamos as
/// sessões do intervalo pedido e fazemos o agrupamento por hora/dia EM MEMÓRIA,
/// já convertendo para o horário local (offset configurável). O volume é pequeno
/// (uma máquina, um dia/semana), então isso é eficiente e evita depender de
/// funções de fuso específicas do banco.
/// </summary>
public sealed class ConsumptionQueries : IConsumptionQueries
{
    private readonly CafeTrackerDbContext _db;
    private readonly TimeSpan _offset;

    public ConsumptionQueries(CafeTrackerDbContext db, IOptions<AppQuerySettings> settings)
    {
        _db = db;
        _offset = TimeSpan.FromHours(settings.Value.TimeZoneOffsetHours);
    }

    // ---- US-03: consumo por hora -------------------------------------------------

    public async Task<IReadOnlyList<HourlyConsumptionDto>> GetHourlyAsync(
        string machineCode, DateOnly date, CancellationToken ct = default)
    {
        var start = LocalMidnight(date);
        var sessions = await SessionsInRangeAsync(machineCode, start, start.AddDays(1), ct);
        var now = DateTimeOffset.Now;

        var activations = new int[24];
        var seconds = new long[24];

        foreach (var s in sessions)
        {
            var hour = ToLocal(s.StartedAt).Hour;
            activations[hour]++;
            seconds[hour] += EffectiveSeconds(s, now);
        }

        return Enumerable.Range(0, 24)
            .Select(h => new HourlyConsumptionDto(h, activations[h], seconds[h]))
            .ToList();
    }

    // ---- US-04: consumo diário e semanal -----------------------------------------

    public async Task<DailyConsumptionDto> GetDailyAsync(
        string machineCode, DateOnly date, CancellationToken ct = default)
    {
        var start = LocalMidnight(date);
        var sessions = await SessionsInRangeAsync(machineCode, start, start.AddDays(1), ct);
        var now = DateTimeOffset.Now;

        return new DailyConsumptionDto(
            date,
            sessions.Count,
            sessions.Sum(s => EffectiveSeconds(s, now)));
    }

    public async Task<IReadOnlyList<DailyConsumptionDto>> GetWeeklyAsync(
        string machineCode, DateOnly anyDateInWeek, CancellationToken ct = default)
    {
        var monday = StartOfWeek(anyDateInWeek);
        var start = LocalMidnight(monday);
        var sessions = await SessionsInRangeAsync(machineCode, start, start.AddDays(7), ct);
        var now = DateTimeOffset.Now;

        // Agrupa por data local.
        var byDate = sessions
            .GroupBy(s => DateOnly.FromDateTime(ToLocal(s.StartedAt).Date))
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Seconds: g.Sum(s => EffectiveSeconds(s, now))));

        // Preenche os 7 dias (segunda → domingo), com zeros onde não houver dados.
        return Enumerable.Range(0, 7)
            .Select(i => monday.AddDays(i))
            .Select(d => byDate.TryGetValue(d, out var v)
                ? new DailyConsumptionDto(d, v.Count, v.Seconds)
                : new DailyConsumptionDto(d, 0, 0))
            .ToList();
    }

    // ---- US-05: horário de pico --------------------------------------------------

    public async Task<PeakHourDto> GetPeakHourAsync(
        string machineCode, DateOnly date, CancellationToken ct = default)
    {
        var hourly = await GetHourlyAsync(machineCode, date, ct);

        // Maior tempo acumulado; empate → hora mais cedo (regra provisória, ver historias.md US-05).
        var best = hourly
            .Where(h => h.TotalSeconds > 0)
            .OrderByDescending(h => h.TotalSeconds)
            .ThenBy(h => h.Hour)
            .FirstOrDefault();

        return best is null ? new PeakHourDto(null, 0) : new PeakHourDto(best.Hour, best.TotalSeconds);
    }

    // ---- US-06: média diária e tendência -----------------------------------------

    public async Task<TrendDto> GetTrendAsync(
        string machineCode, int days, CancellationToken ct = default)
    {
        if (days < 2) days = 2;
        if (days > 31) days = 31;

        var today = LocalToday();
        var firstDay = today.AddDays(-(days - 1));
        var start = LocalMidnight(firstDay);
        var sessions = await SessionsInRangeAsync(machineCode, start, LocalMidnight(today).AddDays(1), ct);
        var now = DateTimeOffset.Now;

        var byDate = sessions
            .GroupBy(s => DateOnly.FromDateTime(ToLocal(s.StartedAt).Date))
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Seconds: g.Sum(s => EffectiveSeconds(s, now))));

        var daysList = Enumerable.Range(0, days)
            .Select(i => firstDay.AddDays(i))
            .Select(d => byDate.TryGetValue(d, out var v)
                ? new DailyConsumptionDto(d, v.Count, v.Seconds)
                : new DailyConsumptionDto(d, 0, 0))
            .ToList();

        var average = daysList.Count == 0 ? 0 : daysList.Average(d => (double)d.TotalSeconds);

        return new TrendDto(average, ClassifyTrend(daysList), daysList);
    }

    // ---- Métrica derivada: acionamentos de hoje ----------------------------------

    public async Task<ActivationsTodayDto> GetActivationsTodayAsync(
        string machineCode, CancellationToken ct = default)
    {
        var today = LocalToday();
        var daily = await GetDailyAsync(machineCode, today, ct);
        return new ActivationsTodayDto(today, daily.Activations);
    }

    // ---- Auxiliares --------------------------------------------------------------

    private Task<List<CoffeeSession>> SessionsInRangeAsync(
        string machineCode, DateTimeOffset startInclusive, DateTimeOffset endExclusive, CancellationToken ct)
        => _db.CoffeeSessions
            .AsNoTracking()
            .Where(s => s.MachineCode == machineCode
                        && s.StartedAt >= startInclusive
                        && s.StartedAt < endExclusive)
            .ToListAsync(ct);

    /// <summary>Tempo de uso da sessão. Aberta → conta até agora; fechada → duração gravada.</summary>
    private static long EffectiveSeconds(CoffeeSession s, DateTimeOffset now)
        => s.DurationSeconds ?? (long)Math.Max(0, (now - s.StartedAt).TotalSeconds);

    private DateTimeOffset ToLocal(DateTimeOffset value) => value.ToOffset(_offset);

    /// <summary>Meia-noite local da data informada, como instante absoluto.</summary>
    private DateTimeOffset LocalMidnight(DateOnly date)
        => new(date.ToDateTime(TimeOnly.MinValue), _offset);

    private DateOnly LocalToday() => DateOnly.FromDateTime(ToLocal(DateTimeOffset.Now).Date);

    private static DateOnly StartOfWeek(DateOnly date)
    {
        // Semana de segunda a domingo (ISO).
        int dow = (int)date.DayOfWeek;          // domingo = 0 ... sábado = 6
        int diffToMonday = dow == 0 ? 6 : dow - 1;
        return date.AddDays(-diffToMonday);
    }

    /// <summary>
    /// Classifica a tendência comparando a média da primeira metade do período
    /// com a da segunda metade. Diferença pequena → "Estável".
    /// </summary>
    private static string ClassifyTrend(IReadOnlyList<DailyConsumptionDto> days)
    {
        if (days.Count < 2)
            return "Estável";

        int half = days.Count / 2;
        double firstAvg = days.Take(half).Select(d => (double)d.TotalSeconds).DefaultIfEmpty(0).Average();
        double secondAvg = days.Skip(days.Count - half).Select(d => (double)d.TotalSeconds).DefaultIfEmpty(0).Average();

        double diff = secondAvg - firstAvg;
        double threshold = Math.Max(60, firstAvg * 0.1); // 10% ou 1 min de folga

        if (diff > threshold) return "Aumentando";
        if (diff < -threshold) return "Diminuindo";
        return "Estável";
    }
}
