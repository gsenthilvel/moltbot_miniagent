using MiniAgent.Agent;
using MiniAgent.Configuration;
using MiniAgent.LLM;
using MiniAgent.Skills;
using MiniAgent.Tools;

var config = ConfigLoader.Load(null, args);
var workspaceRoot = Directory.GetCurrentDirectory();

var skillLoader = new SkillLoader(workspaceRoot, config.Skills.ExtraDirs);
var skills = skillLoader.Load();

var toolRegistry = new ToolRegistry();
toolRegistry.Register(new FileReadTool());
toolRegistry.Register(new FileWriteTool());
toolRegistry.Register(new ShellTool());

using var httpClient = new HttpClient();
httpClient.Timeout = TimeSpan.FromSeconds(config.Agent.TimeoutSeconds + 30);

ILlmProvider llm = config.Provider.ToLowerInvariant() switch
{
    "openai" => new OpenAiProvider(httpClient, config.OpenAI),
    _ => new OllamaProvider(httpClient, config.Ollama)
};

var promptBuilder = new SystemPromptBuilder();
var session = new SessionManager();
var agentLoop = new AgentLoop(
    llm,
    toolRegistry,
    promptBuilder,
    config.Agent.MaxToolRounds,
    config.Agent.TimeoutSeconds);

Console.WriteLine("MiniAgent (lightweight AI agent). Commands: /exit to quit, /clear to clear session.");
Console.WriteLine($"Provider: {config.Provider}, Skills loaded: {skills.Count}");
Console.WriteLine();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim() ?? "";
    if (string.IsNullOrEmpty(input)) continue;
    if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase)) break;
    if (input.Equals("/clear", StringComparison.OrdinalIgnoreCase))
    {
        session.Clear();
        Console.WriteLine("Session cleared.");
        continue;
    }

    try
    {
        await agentLoop.RunAsync(session, skills, input);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
