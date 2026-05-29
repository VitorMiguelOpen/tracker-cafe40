namespace CafeTracker.Application.Queries;

/// <summary>
/// Porta de leitura do status atual (lado de consulta). A implementação concreta
/// (EF Core) vive na Infrastructure — a Api só conhece este contrato.
/// </summary>
public interface IStatusQueries
{
    /// <summary>Último status conhecido da máquina, ou null se ainda não há eventos.</summary>
    Task<CurrentStatusDto?> GetCurrentAsync(string machineCode, CancellationToken ct = default);
}
