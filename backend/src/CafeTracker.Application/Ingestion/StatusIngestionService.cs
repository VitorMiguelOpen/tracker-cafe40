using CafeTracker.Application.Abstractions;
using CafeTracker.Application.Realtime;
using CafeTracker.Domain.Entities;
using CafeTracker.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CafeTracker.Application.Ingestion;

/// <summary>
/// Coração da aplicação: recebe cada leitura do MQTT e aplica a REGRA DE TRANSIÇÃO.
///
/// Como o valor se repete no MQTT (publicado periodicamente, não só na mudança),
/// só agimos quando o valor REALMENTE muda em relação ao último conhecido:
///   • 0 → 1 : abre uma <see cref="CoffeeSession"/> (1 acionamento — US-07)
///   • 1 → 0 : fecha a sessão aberta (calcula a duração)
/// Em ambos os casos grava o <see cref="StatusEvent"/> (trilha de auditoria).
/// </summary>
public sealed class StatusIngestionService
{
    private readonly IStatusEventRepository _events;
    private readonly ICoffeeSessionRepository _sessions;
    private readonly IRealtimeNotifier _notifier;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StatusIngestionService> _logger;

    public StatusIngestionService(
        IStatusEventRepository events,
        ICoffeeSessionRepository sessions,
        IRealtimeNotifier notifier,
        IUnitOfWork unitOfWork,
        ILogger<StatusIngestionService> logger)
    {
        _events = events;
        _sessions = sessions;
        _notifier = notifier;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Processa uma leitura vinda do MQTT.</summary>
    public async Task IngestAsync(StatusReading reading, CancellationToken ct = default)
    {
        // Horário local do servidor (decisão de fuso: trabalhamos em horário local).
        var receivedAt = DateTimeOffset.Now;

        // Busca o último valor conhecido (no banco) — funciona mesmo após reiniciar.
        var last = await _events.GetLastAsync(reading.MachineCode, ct);

        // 1) Valor NÃO mudou → ignora (evita duplicado, pois o valor se repete).
        if (last is not null && last.Value == reading.Value)
        {
            _logger.LogDebug(
                "Valor repetido ({Value}) para {Machine} — ignorado.",
                reading.Value, reading.MachineCode);
            return;
        }

        // 2) Valor MUDOU → grava o evento de transição (append-only).
        var statusEvent = new StatusEvent(
            reading.MachineCode,
            reading.Tag,
            reading.Value,
            reading.EventTime,
            receivedAt,
            reading.RawPayload);

        await _events.AddAsync(statusEvent, ct);

        // 3) Aplica a regra de sessão conforme a direção da transição.
        if (reading.Value == (short)MachineState.Brewing)
        {
            // 0 → 1 : começou a fazer café. Abre sessão (se já não houver uma aberta).
            var open = await _sessions.GetOpenAsync(reading.MachineCode, ct);
            if (open is null)
            {
                await _sessions.AddAsync(
                    CoffeeSession.Open(reading.MachineCode, reading.EventTime), ct);
                _logger.LogInformation(
                    "Sessão ABERTA para {Machine} em {Time}.",
                    reading.MachineCode, reading.EventTime);
            }
        }
        else if (reading.Value == (short)MachineState.Stopped)
        {
            // 1 → 0 : parou. Fecha a sessão aberta (se houver).
            var open = await _sessions.GetOpenAsync(reading.MachineCode, ct);
            if (open is not null)
            {
                open.Close(reading.EventTime);
                _sessions.Update(open);
                _logger.LogInformation(
                    "Sessão FECHADA para {Machine}: {Duration}s.",
                    reading.MachineCode, open.DurationSeconds);
            }
        }

        // 4) Confirma tudo de uma vez (atômico) e notifica o dashboard.
        await _unitOfWork.SaveChangesAsync(ct);

        await _notifier.NotifyStatusChangedAsync(
            new StatusChangedNotification(
                reading.MachineCode,
                reading.Value,
                (MachineState)reading.Value,
                reading.EventTime),
            ct);
    }
}
