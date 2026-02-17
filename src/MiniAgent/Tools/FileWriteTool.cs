using System.Text.Json;

namespace MiniAgent.Tools;

public class FileWriteTool : ITool
{
    public string Name => "file_write";
    public string Description => "Write or overwrite a file on disk with the given content.";

    public JsonElement ParametersSchema => JsonSerializer.SerializeToElement(new
    {
        type = "object",
        required = new[] { "path", "content" },
        properties = new
        {
            path = new { type = "string", description = "Path to the file to write" },
            content = new { type = "string", description = "Content to write to the file" }
        }
    });

    public Task<string> ExecuteAsync(JsonElement arguments, CancellationToken ct = default)
    {
        var path = arguments.GetProperty("path").GetString() ?? "";
        var content = arguments.GetProperty("content").GetString() ?? "";
        if (string.IsNullOrWhiteSpace(path))
            return Task.FromResult("{\"error\":\"path is required\"}");
        try
        {
            var fullPath = Path.GetFullPath(path);
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(fullPath, content);
            return Task.FromResult(JsonSerializer.Serialize(new { path = fullPath, written = true }));
        }
        catch (Exception ex)
        {
            return Task.FromResult($"{{\"error\":\"{ex.Message}\"}}");
        }
    }
}
