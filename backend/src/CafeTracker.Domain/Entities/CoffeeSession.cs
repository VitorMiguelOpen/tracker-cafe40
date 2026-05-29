namespace CafeTracker.Domain.Entities;

/// <summary>
/// Sessão de uso da máquina: um ciclo completo "ligou → desligou".
/// Cada sessão equivale a 1 acionamento (US-07). Abre na transição 0→1
/// e fecha na transição 1→0. Corresponde à tabela `coffee_session` (docs/banco.md).
/// </summary>
public sealed class CoffeeSession
{
    /// <summary>Chave primária (gerada pelo banco).</summary>
    public long Id { get; private set; }

    /// <summary>Código da máquina (ex.: "SAACE").</summary>
    public string MachineCode { get; private set; } = null!;

    /// <summary>Início do ciclo — momento da transição 0 → 1.</summary>
    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>Fim do ciclo — momento da transição 1 → 0. Null enquanto em andamento.</summary>
    public DateTimeOffset? EndedAt { get; private set; }

    /// <summary>Duração em segundos, preenchida ao fechar a sessão.</summary>
    public int? DurationSeconds { get; private set; }

    /// <summary>True enquanto a sessão não foi fechada (máquina ainda fazendo café).</summary>
    public bool IsOpen { get; private set; }

    // Construtor sem parâmetros exigido pelo EF Core.
    private CoffeeSession() { }

    /// <summary>Abre uma nova sessão (transição 0 → 1).</summary>
    public static CoffeeSession Open(string machineCode, DateTimeOffset startedAt)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
            throw new ArgumentException("MachineCode é obrigatório.", nameof(machineCode));

        return new CoffeeSession
        {
            MachineCode = machineCode,
            StartedAt = startedAt,
            IsOpen = true
        };
    }

    /// <summary>Fecha a sessão (transição 1 → 0), calculando a duração.</summary>
    public void Close(DateTimeOffset endedAt)
    {
        if (!IsOpen)
            throw new InvalidOperationException("A sessão já está fechada.");

        if (endedAt < StartedAt)
            throw new ArgumentException("O fim não pode ser anterior ao início.", nameof(endedAt));

        EndedAt = endedAt;
        DurationSeconds = (int)(endedAt - StartedAt).TotalSeconds;
        IsOpen = false;
    }
}
