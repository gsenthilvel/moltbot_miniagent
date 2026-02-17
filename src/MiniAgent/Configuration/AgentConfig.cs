namespace MiniAgent.Configuration;

public class AgentConfig
{
    public string Provider { get; set; } = "ollama";
    public OllamaConfig Ollama { get; set; } = new();
    public OpenAIConfig OpenAI { get; set; } = new();
    public SkillsConfig Skills { get; set; } = new();
    public AgentSectionConfig Agent { get; set; } = new();
}

public class OllamaConfig
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "qwen2.5:7b";
}

public class OpenAIConfig
{
    public string Endpoint { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-4o-mini";
    public string ApiKey { get; set; } = "";
}

public class SkillsConfig
{
    public List<string> ExtraDirs { get; set; } = new();
    public Dictionary<string, SkillEntryConfig> Entries { get; set; } = new();
}

public class SkillEntryConfig
{
    public bool Enabled { get; set; } = true;
    public string? ApiKey { get; set; }
    public Dictionary<string, string>? Env { get; set; }
    public Dictionary<string, object?>? Config { get; set; }
}

public class AgentSectionConfig
{
    public int TimeoutSeconds { get; set; } = 300;
    public int MaxToolRounds { get; set; } = 10;
}
