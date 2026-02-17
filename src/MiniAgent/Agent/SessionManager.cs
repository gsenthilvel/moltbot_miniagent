using MiniAgent.LLM;

namespace MiniAgent.Agent;

public class SessionManager
{
    private readonly List<LlmMessage> _messages = new();

    public IReadOnlyList<LlmMessage> Messages => _messages;

    public void AddUserMessage(string content)
    {
        _messages.Add(new LlmMessage("user", content));
    }

    public void AddAssistantMessage(string content, IReadOnlyList<LlmToolCall>? toolCalls = null)
    {
        _messages.Add(new LlmMessage("assistant", content, toolCalls));
    }

    public void AddToolResult(string toolCallId, string content)
    {
        _messages.Add(new LlmMessage("tool", content, null, toolCallId));
    }

    public void Clear()
    {
        _messages.Clear();
    }
}
