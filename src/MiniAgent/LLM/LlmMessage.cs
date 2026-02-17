using System.Text.Json;

namespace MiniAgent.LLM;

public record LlmMessage(string Role, string Content, IReadOnlyList<LlmToolCall>? ToolCalls = null, string? ToolCallId = null);

public record LlmToolCall(string Id, string Name, JsonElement Arguments);

public record LlmResponse(string Content, IReadOnlyList<LlmToolCall> ToolCalls, string? StopReason = null)
{
    public bool HasToolCalls => ToolCalls.Count > 0;
}

public record ToolDefinition(string Name, string Description, JsonElement ParametersSchema);
