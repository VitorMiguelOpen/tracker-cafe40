using CafeTracker.Domain.Entities;

namespace CafeTracker.Application.Abstractions;

/// <summary>
/// Porta de persistência das sessões de uso (`coffee_session`).
/// </summary>
public interface ICoffeeSessionRepository
{
    /// <summary>
    /// Sessão atualmente aberta da máquina (IsOpen = true), se existir.
    /// Usada para fechar na transição 1→0 e para evitar abrir duas ao mesmo tempo.
    /// </summary>
    Task<CoffeeSession?> GetOpenAsync(string machineCode, CancellationToken ct = default);

    /// <summary>Registra uma nova sessão (transição 0→1).</summary>
    Task AddAsync(CoffeeSession session, CancellationToken ct = default);

    /// <summary>Marca uma sessão existente como alterada (ao fechar, transição 1→0).</summary>
    void Update(CoffeeSession session);
}
