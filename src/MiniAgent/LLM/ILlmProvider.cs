namespace MiniAgent.LLM;

public interface ILlmProvider
{
    Task<LlmResponse> ChatAsync(
        string systemPrompt,
        IReadOnlyList<LlmMessage> messages,
        IReadOnlyList<ToolDefinition> tools,
        CancellationToken ct = default);
}
