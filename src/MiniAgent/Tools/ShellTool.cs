using System.Diagnostics;
using System.Text.Json;

namespace MiniAgent.Tools;

public class ShellTool : ITool
{
    public string Name => "shell_exec";
    public string Description => "Execute a shell command and return stdout and stderr. Use for running terminal commands.";

    public JsonElement ParametersSchema => JsonSerializer.SerializeToElement(new
    {
        type = "object",
        required = new[] { "command" },
        properties = new
        {
            command = new { type = "string", description = "The shell command to execute" }
        }
    });

    public async Task<string> ExecuteAsync(JsonElement arguments, CancellationToken ct = default)
    {
        var command = arguments.GetProperty("command").GetString() ?? "";
        if (string.IsNullOrWhiteSpace(command))
            return "{\"error\":\"command is required\"}";

        try
        {
            var isWindows = OperatingSystem.IsWindows();
            var psi = new ProcessStartInfo
            {
                FileName = isWindows ? "cmd.exe" : "/bin/sh",
                Arguments = isWindows ? $"/c \"{command.Replace("\"", "\\\"")}\"" : "-c " + EscapeShell(command),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null)
                return "{\"error\":\"Failed to start process\"}";
            var stdout = await process.StandardOutput.ReadToEndAsync(ct);
            var stderr = await process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);
            return JsonSerializer.Serialize(new
            {
                exitCode = process.ExitCode,
                stdout,
                stderr
            });
        }
        catch (Exception ex)
        {
            return $"{{\"error\":\"{ex.Message}\"}}";
        }
    }

    private static string EscapeShell(string s)
    {
        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$") + "\"";
    }
}
