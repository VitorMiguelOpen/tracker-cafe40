using CafeTracker.Domain.Entities;

namespace CafeTracker.Application.Abstractions;

/// <summary>
/// Porta de persistência do log de transições (`status_event`).
/// A implementação concreta (EF Core/Postgres) vive na camada Infrastructure.
/// </summary>
public interface IStatusEventRepository
{
    /// <summary>
    /// Último evento conhecido da máquina (por EventTime). É o que permite
    /// saber o "valor anterior" mesmo após reiniciar o backend (robustez US-01).
    /// </summary>
    Task<StatusEvent?> GetLastAsync(string machineCode, CancellationToken ct = default);

    /// <summary>Registra um novo evento de transição.</summary>
    Task AddAsync(StatusEvent statusEvent, CancellationToken ct = default);
}
