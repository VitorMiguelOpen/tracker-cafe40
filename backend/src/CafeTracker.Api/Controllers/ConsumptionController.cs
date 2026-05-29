using CafeTracker.Application.Queries;
using CafeTracker.Infrastructure.Persistence.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CafeTracker.Api.Controllers;

/// <summary>
/// Endpoints analíticos de consumo (US-03 a US-06) + métrica de acionamentos.
/// As datas usam o formato ISO (YYYY-MM-DD) e são interpretadas no fuso local
/// configurado. Quando a data é omitida, assume-se o dia de hoje (local).
/// </summary>
[ApiController]
[Route("api/consumption")]
public sealed class ConsumptionController : ControllerBase
{
    private readonly IConsumptionQueries _consumption;
    private readonly AppQuerySettings _settings;

    public ConsumptionController(IConsumptionQueries consumption, IOptions<AppQuerySettings> settings)
    {
        _consumption = consumption;
        _settings = settings.Value;
    }

    /// <summary>US-03: consumo por hora (24 faixas) do dia informado.</summary>
    [HttpGet("hourly")]
    public async Task<ActionResult<IReadOnlyList<HourlyConsumptionDto>>> GetHourly(
        [FromQuery] DateOnly? date, [FromQuery] string? machine, CancellationToken ct)
        => Ok(await _consumption.GetHourlyAsync(Machine(machine), date ?? Today(), ct));

    /// <summary>US-04: consumo de um único dia.</summary>
    [HttpGet("daily")]
    public async Task<ActionResult<DailyConsumptionDto>> GetDaily(
        [FromQuery] DateOnly? date, [FromQuery] string? machine, CancellationToken ct)
        => Ok(await _consumption.GetDailyAsync(Machine(machine), date ?? Today(), ct));

    /// <summary>US-04: consumo da semana (seg→dom) que contém a data informada.</summary>
    [HttpGet("weekly")]
    public async Task<ActionResult<IReadOnlyList<DailyConsumptionDto>>> GetWeekly(
        [FromQuery] DateOnly? date, [FromQuery] string? machine, CancellationToken ct)
        => Ok(await _consumption.GetWeeklyAsync(Machine(machine), date ?? Today(), ct));

    /// <summary>US-05: horário de pico (maior tempo acumulado) do dia informado.</summary>
    [HttpGet("peak")]
    public async Task<ActionResult<PeakHourDto>> GetPeak(
        [FromQuery] DateOnly? date, [FromQuery] string? machine, CancellationToken ct)
        => Ok(await _consumption.GetPeakHourAsync(Machine(machine), date ?? Today(), ct));

    /// <summary>US-06: média diária e tendência nos últimos N dias (padrão 7).</summary>
    [HttpGet("trend")]
    public async Task<ActionResult<TrendDto>> GetTrend(
        [FromQuery] int days, [FromQuery] string? machine, CancellationToken ct)
        => Ok(await _consumption.GetTrendAsync(Machine(machine), days <= 0 ? 7 : days, ct));

    /// <summary>Métrica derivada: total de acionamentos de hoje.</summary>
    [HttpGet("activations/today")]
    public async Task<ActionResult<ActivationsTodayDto>> GetActivationsToday(
        [FromQuery] string? machine, CancellationToken ct)
        => Ok(await _consumption.GetActivationsTodayAsync(Machine(machine), ct));

    // ---- Auxiliares --------------------------------------------------------------

    private string Machine(string? machine)
        => string.IsNullOrWhiteSpace(machine) ? _settings.MachineCode : machine;

    private DateOnly Today()
    {
        var offset = TimeSpan.FromHours(_settings.TimeZoneOffsetHours);
        return DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(offset).Date);
    }
}
