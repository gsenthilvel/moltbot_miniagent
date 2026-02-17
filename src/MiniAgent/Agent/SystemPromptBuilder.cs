using System.Text;
using MiniAgent.LLM;
using MiniAgent.Skills;
using MiniAgent.Tools;

namespace MiniAgent.Agent;

public class SystemPromptBuilder
{
    private const string BasePrompt = @"You are MiniAgent, a helpful AI assistant. You can use tools to read/write files and run shell commands.
When you need to use a tool, respond with the appropriate tool call. After receiving tool results, summarize or continue as needed.
Be concise and accurate. If a tool fails, explain what went wrong and suggest a fix if possible.";

    public string Build(IReadOnlyList<Skill> skills, IReadOnlyList<LlmMessage> sessionMessages, IReadOnlyList<ToolDefinition> tools)
    {
        var sb = new StringBuilder(BasePrompt);
        sb.AppendLine("\n\n## Available skills\n");
        foreach (var s in skills)
        {
            sb.AppendLine($"<skill name=\"{EscapeXml(s.Name)}\" description=\"{EscapeXml(s.Description)}\" location=\"{EscapeXml(s.Location)}\"/>");
            if (!string.IsNullOrWhiteSpace(s.Instructions))
            {
                sb.AppendLine();
                sb.AppendLine(s.Instructions.Trim());
                sb.AppendLine();
            }
        }
        sb.AppendLine("\n## Available tools\n");
        foreach (var t in tools)
            sb.AppendLine($"- {t.Name}: {t.Description}");
        return sb.ToString();
    }

    private static string EscapeXml(string s)
    {
        return s
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
