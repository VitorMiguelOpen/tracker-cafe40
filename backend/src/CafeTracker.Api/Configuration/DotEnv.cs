namespace CafeTracker.Api.Configuration;

/// <summary>
/// Carregador simples de arquivo <c>.env</c> (sem dependência externa).
///
/// Procura o arquivo subindo a árvore de diretórios a partir do diretório de
/// execução (no dev, o .env fica na raiz do repositório, fora do versionamento).
/// Cada linha "CHAVE=valor" vira uma variável de ambiente do processo — assim os
/// segredos (usuário/senha MQTT, connection string) NUNCA ficam no código nem no
/// appsettings de um repositório público.
///
/// IMPORTANTE: chamar ANTES de WebApplication.CreateBuilder, para que variáveis
/// como ASPNETCORE_URLS e DB_CONNECTION_STRING já estejam visíveis ao host.
/// </summary>
public static class DotEnv
{
    public static void Load(string fileName = ".env")
    {
        var path = FindUpwards(fileName);
        if (path is null)
            return; // sem .env (ex.: produção usa variáveis de ambiente reais) — ok.

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();

            // Ignora vazios e comentários.
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = line.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"');

            // Não sobrescreve variáveis já definidas no ambiente real (produção vence).
            if (Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string? FindUpwards(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, fileName);
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }
}
