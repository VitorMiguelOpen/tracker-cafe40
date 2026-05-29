namespace CafeTracker.Application.Abstractions;

/// <summary>
/// Confirma, de forma atômica, todas as alterações feitas pelos repositórios
/// dentro de uma mesma operação (padrão Unit of Work). Com EF Core, a
/// implementação delega ao DbContext.SaveChanges.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
