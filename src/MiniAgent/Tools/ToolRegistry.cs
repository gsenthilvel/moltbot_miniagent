using System.Text.Json;
using MiniAgent.LLM;

namespace MiniAgent.Tools;

public class ToolRegistry
{
    private static string EscapeJson(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    private readonly Dictionary<string, ITool> _tools = new();

    public void Register(ITool tool)
    {
        _tools[tool.Name] = tool;
    }

    public IReadOnlyList<ToolDefinition> GetDefinitions()
    {
        return _tools.Values.Select(t => new ToolDefinition(t.Name, t.Description, t.ParametersSchema)).ToList();
    }

    public async Task<string> ExecuteAsync(string name, JsonElement arguments, CancellationToken ct = default)
    {
        if (!_tools.TryGetValue(name, out var tool))
            return $"{{\"error\":\"Unknown tool: {name}\"}}";
        try
        {
            return await tool.ExecuteAsync(arguments, ct);
        }
        catch (Exception ex)
        {
            return $"{{\"error\":\"{EscapeJson(ex.Message)}\"}}";
        }
    }
}
