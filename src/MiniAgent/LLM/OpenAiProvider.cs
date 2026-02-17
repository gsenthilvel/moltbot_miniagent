using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MiniAgent.Configuration;

namespace MiniAgent.LLM;

public class OpenAiProvider : ILlmProvider
{
    private readonly HttpClient _client;
    private readonly OpenAIConfig _config;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public OpenAiProvider(HttpClient client, OpenAIConfig config)
    {
        _client = client;
        _client.BaseAddress = new Uri(config.Endpoint.TrimEnd('/') + "/");
        _config = config;
        if (!string.IsNullOrEmpty(config.ApiKey))
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiKey);
    }

    public async Task<LlmResponse> ChatAsync(
        string systemPrompt,
        IReadOnlyList<LlmMessage> messages,
        IReadOnlyList<ToolDefinition> tools,
        CancellationToken ct = default)
    {
        var messageList = new List<object>
        {
            new Dictionary<string, string> { ["role"] = "system", ["content"] = systemPrompt }
        };

        foreach (var m in messages)
        {
            if (m.Role == "user")
                messageList.Add(new Dictionary<string, string> { ["role"] = "user", ["content"] = m.Content ?? "" });
            else if (m.Role == "assistant")
            {
                var assistant = new Dictionary<string, object> { ["role"] = "assistant", ["content"] = m.Content ?? "" };
                if (m.ToolCalls is { Count: > 0 } tc)
                    assistant["tool_calls"] = tc.Select(t => new Dictionary<string, object>
                    {
                        ["id"] = t.Id,
                        ["type"] = "function",
                        ["function"] = new Dictionary<string, object> { ["name"] = t.Name, ["arguments"] = t.Arguments.GetRawText() ?? "{}" }
                    }).ToList();
                messageList.Add(assistant);
            }
            else if (m.Role == "tool")
            {
                messageList.Add(new Dictionary<string, object>
                {
                    ["role"] = "tool",
                    ["tool_call_id"] = m.ToolCallId ?? "",
                    ["content"] = m.Content ?? ""
                });
            }
        }

        var body = new Dictionary<string, object>
        {
            ["model"] = _config.Model,
            ["messages"] = messageList,
            ["tools"] = tools.Select(t => new Dictionary<string, object>
            {
                ["type"] = "function",
                ["function"] = new Dictionary<string, object>
                {
                    ["name"] = t.Name,
                    ["description"] = t.Description,
                    ["parameters"] = JsonSerializer.Deserialize<JsonElement>(t.ParametersSchema.GetRawText())
                }
            }).ToList()
        };

        const int maxRetries = 5;
        const int baseDelayMs = 5000;
        string? lastErrorBody = null;
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var delaySec = (baseDelayMs * (1 << (attempt - 1))) / 1000;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  [Rate limited] Retrying in {delaySec}s (attempt {attempt + 1}/{maxRetries})...");
                Console.ResetColor();
                await Task.Delay(delaySec * 1000, ct);
            }
            using var reqContent = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
            using var response = await _client.PostAsync("chat/completions", reqContent, ct);

            if ((int)response.StatusCode == 429)
            {
                lastErrorBody = await response.Content.ReadAsStringAsync(ct);
                if (attempt < maxRetries - 1) continue;
                throw new HttpRequestException(
                    $"OpenAI rate limit (429) after {maxRetries} retries. " +
                    "Check your API key billing/quota at https://platform.openai.com/usage\n" +
                    $"Response: {Truncate(lastErrorBody, 300)}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException(
                    $"OpenAI error {(int)response.StatusCode}: {Truncate(errBody, 300)}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, ct);
            var choice = json.GetProperty("choices")[0];
            var msg = choice.GetProperty("message");
            var contentStr = msg.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "";
            var toolCalls = new List<LlmToolCall>();
            if (msg.TryGetProperty("tool_calls", out var tcArr))
            {
                foreach (var tc in tcArr.EnumerateArray())
                {
                    var id = tc.GetProperty("id").GetString() ?? "";
                    var fn = tc.GetProperty("function");
                    var name = fn.GetProperty("name").GetString() ?? "";
                    var argsStr = fn.TryGetProperty("arguments", out var a) ? a.GetString() ?? "{}" : "{}";
                    var args = JsonSerializer.Deserialize<JsonElement>(argsStr);
                    toolCalls.Add(new LlmToolCall(id, name, args));
                }
            }

            return new LlmResponse(contentStr, toolCalls, choice.TryGetProperty("finish_reason", out var fr) ? fr.GetString() : null);
        }

        throw new HttpRequestException("OpenAI returned 429 after all retries.");
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "...";
}
