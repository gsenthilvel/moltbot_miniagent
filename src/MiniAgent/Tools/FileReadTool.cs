using System.Text.Json;

namespace MiniAgent.Tools;

public class FileReadTool : ITool
{
    public string Name => "file_read";
    public string Description => "Read the contents of a file from disk. Use a path relative to the current working directory or an absolute path.";

    public JsonElement ParametersSchema => JsonSerializer.SerializeToElement(new
    {
        type = "object",
        required = new[] { "path" },
        properties = new
        {
            path = new { type = "string", description = "Path to the file to read" }
        }
    });

    public Task<string> ExecuteAsync(JsonElement arguments, CancellationToken ct = default)
    {
        var path = arguments.GetProperty("path").GetString() ?? "";
        if (string.IsNullOrWhiteSpace(path))
            return Task.FromResult("{\"error\":\"path is required\"}");
        try
        {
            var fullPath = Path.GetFullPath(path);
            var content = File.ReadAllText(fullPath);
            return Task.FromResult(JsonSerializer.Serialize(new { path = fullPath, content }));
        }
        catch (Exception ex)
        {
            return Task.FromResult($"{{\"error\":\"{ex.Message}\"}}");
        }
    }
}
