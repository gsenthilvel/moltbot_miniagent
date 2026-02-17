using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MiniAgent.Configuration;

namespace MiniAgent.LLM;

public class OllamaProvider : ILlmProvider
{
    private readonly HttpClient _client;
    private readonly OllamaConfig _config;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public OllamaProvider(HttpClient client, OllamaConfig config)
    {
        _client = client;
        _client.BaseAddress = new Uri(config.Endpoint.TrimEnd('/') + "/");
        _config = config;
    }

    public async Task<LlmResponse> ChatAsync(
        string systemPrompt,
        IReadOnlyList<LlmMessage> messages,
        IReadOnlyList<ToolDefinition> tools,
        CancellationToken ct = default)
    {
        var messageList = new List<Dictionary<string, object>>
        {
            new() { ["role"] = "system", ["content"] = systemPrompt }
        };

        foreach (var m in messages)
        {
            switch (m.Role)
            {
                case "user":
                    messageList.Add(new Dictionary<string, object> { ["role"] = "user", ["content"] = m.Content ?? "" });
                    break;
                case "assistant":
                    var assistant = new Dictionary<string, object> { ["role"] = "assistant", ["content"] = m.Content ?? "" };
                    if (m.ToolCalls is { Count: > 0 } tc)
                        assistant["tool_calls"] = tc.Select(t => new Dictionary<string, object>
                        {
                            ["function"] = new Dictionary<string, object>
                            {
                                ["name"] = t.Name,
                                ["arguments"] = t.Arguments.ValueKind == JsonValueKind.String
                                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(t.Arguments.GetString()!)!
                                    : JsonSerializer.Deserialize<Dictionary<string, object>>(t.Arguments.GetRawText())!
                            }
                        }).ToList();
                    messageList.Add(assistant);
                    break;
                case "tool":
                    messageList.Add(new Dictionary<string, object> { ["role"] = "tool", ["content"] = m.Content ?? "" });
                    break;
            }
        }

        var toolsPayload = tools.Select(t => new Dictionary<string, object>
        {
            ["type"] = "function",
            ["function"] = new Dictionary<string, object>
            {
                ["name"] = t.Name,
                ["description"] = t.Description,
                ["parameters"] = JsonSerializer.Deserialize<JsonElement>(t.ParametersSchema.GetRawText())
            }
        }).ToList();

        var body = new Dictionary<string, object>
        {
            ["model"] = _config.Model,
            ["stream"] = false,
            ["messages"] = messageList,
            ["tools"] = toolsPayload
        };

        var jsonStr = JsonSerializer.Serialize(body, JsonOptions);
        using var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync("api/chat", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Ollama error {(int)response.StatusCode}: {(errBody.Length > 500 ? errBody[..500] : errBody)}");
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, ct);
        var message = json.GetProperty("message");
        var contentStr = message.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "";
        var toolCalls = new List<LlmToolCall>();
        if (message.TryGetProperty("tool_calls", out var tcArr))
        {
            foreach (var tc in tcArr.EnumerateArray())
            {
                var fn = tc.GetProperty("function");
                var id = tc.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? Guid.NewGuid().ToString("N") : Guid.NewGuid().ToString("N");
                var name = fn.GetProperty("name").GetString() ?? "";
                var args = fn.TryGetProperty("arguments", out var a) ? a : JsonSerializer.SerializeToElement("{}");
                toolCalls.Add(new LlmToolCall(id, name, args));
            }
        }

        return new LlmResponse(contentStr, toolCalls, json.TryGetProperty("done_reason", out var dr) ? dr.GetString() : null);
    }
}
