using System.Globalization;
using CafeTracker.Application.Ingestion;

namespace CafeTracker.Infrastructure.Messaging;

/// <summary>
/// Converte o payload do tópico DADOSAPONTAMENTO numa <see cref="StatusReading"/>.
/// Formato (string delimitada por '|'):
///   "2026-05-28T15:43:45.000-03:00|99|0|Status (H1)|Machine status signal|"
///        timestamp                 |tag|val|  nome    |     descrição      |
/// </summary>
public static class DadosApontamentoParser
{
    /// <summary>
    /// Tenta interpretar o payload. Retorna false (sem lançar) se o formato
    /// não bater — assim uma mensagem estranha nunca derruba o serviço.
    /// </summary>
    public static bool TryParse(string? payload, string machineCode, out StatusReading? reading)
    {
        reading = null;

        if (string.IsNullOrWhiteSpace(payload))
            return false;

        var parts = payload.Split('|');
        if (parts.Length < 3)
            return false;

        // parts[0] = timestamp ISO 8601 com offset (-03:00)
        if (!DateTimeOffset.TryParse(
                parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var eventTime))
            return false;

        // parts[1] = tag (ex.: 99)
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var tag))
            return false;

        // parts[2] = valor (0 ou 1)
        if (!short.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            return false;

        reading = new StatusReading(machineCode, tag, value, eventTime, payload);
        return true;
    }
}
