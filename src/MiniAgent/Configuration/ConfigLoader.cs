using System.Text.Json;

namespace MiniAgent.Configuration;

public static class ConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static AgentConfig Load(string? configPath = null, string[]? args = null)
    {
        var path = configPath ?? FindConfigPath();
        var config = File.Exists(path)
            ? JsonSerializer.Deserialize<AgentConfig>(File.ReadAllText(path), JsonOptions) ?? new AgentConfig()
            : new AgentConfig();

        ApplyEnvironmentOverrides(config);
        ApplyArgsOverrides(config, args ?? Array.Empty<string>());
        return config;
    }

    private static string FindConfigPath()
    {
        var current = Directory.GetCurrentDirectory();
        var path = Path.Combine(current, "miniagent.json");
        if (File.Exists(path)) return path;

        var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        path = Path.Combine(userHome, ".miniagent", "miniagent.json");
        return path;
    }

    private static void ApplyEnvironmentOverrides(AgentConfig config)
    {
        if (GetEnv("MINIAGENT_PROVIDER") is { } p) config.Provider = p;
        if (GetEnv("OLLAMA_HOST") is { } o) config.Ollama.Endpoint = o.StartsWith("http") ? o : "http://" + o;
        if (GetEnv("OLLAMA_MODEL") is { } m) config.Ollama.Model = m;
        if (GetEnv("OPENAI_API_KEY") is { } k) config.OpenAI.ApiKey = k;
        if (GetEnv("OPENAI_API_BASE") is { } b) config.OpenAI.Endpoint = b;
        if (GetEnv("OPENAI_MODEL") is { } om) config.OpenAI.Model = om;
    }

    private static string? GetEnv(string name)
    {
        var v = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }

    private static void ApplyArgsOverrides(AgentConfig config, string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--provider" && i + 1 < args.Length)
            {
                config.Provider = args[++i];
                continue;
            }
            if (args[i] == "--ollama-model" && i + 1 < args.Length)
            {
                config.Ollama.Model = args[++i];
                continue;
            }
            if (args[i] == "--openai-model" && i + 1 < args.Length)
            {
                config.OpenAI.Model = args[++i];
                continue;
            }
        }
    }
}
