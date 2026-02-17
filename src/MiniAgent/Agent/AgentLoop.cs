using System.Text.Json;
using MiniAgent.LLM;
using MiniAgent.Skills;
using MiniAgent.Tools;

namespace MiniAgent.Agent;

public class AgentLoop
{
    private readonly ILlmProvider _llm;
    private readonly ToolRegistry _tools;
    private readonly SystemPromptBuilder _promptBuilder;
    private readonly int _maxToolRounds;
    private readonly int _timeoutSeconds;

    public AgentLoop(
        ILlmProvider llm,
        ToolRegistry tools,
        SystemPromptBuilder promptBuilder,
        int maxToolRounds = 10,
        int timeoutSeconds = 300)
    {
        _llm = llm;
        _tools = tools;
        _promptBuilder = promptBuilder;
        _maxToolRounds = maxToolRounds;
        _timeoutSeconds = timeoutSeconds;
    }

    public async Task RunAsync(
        SessionManager session,
        IReadOnlyList<Skill> skills,
        string userInput,
        CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

        session.AddUserMessage(userInput);
        var toolDefinitions = _tools.GetDefinitions();
        var systemPrompt = _promptBuilder.Build(skills, session.Messages, toolDefinitions);

        var round = 0;
        LlmResponse response;
        do
        {
            response = await _llm.ChatAsync(systemPrompt, session.Messages, toolDefinitions, cts.Token);

            if (!response.HasToolCalls)
                break;

            session.AddAssistantMessage(response.Content, response.ToolCalls);
            foreach (var tc in response.ToolCalls)
            {
                var args = tc.Arguments.ValueKind == JsonValueKind.String
                    ? JsonSerializer.Deserialize<JsonElement>(tc.Arguments.GetString() ?? "{}")
                    : tc.Arguments;
                var result = await _tools.ExecuteAsync(tc.Name, args, cts.Token);
                session.AddToolResult(tc.Id, result);
            }
            round++;
        } while (round < _maxToolRounds && response.HasToolCalls);

        session.AddAssistantMessage(response.Content, null);
        Console.WriteLine(response.Content);
    }
}
