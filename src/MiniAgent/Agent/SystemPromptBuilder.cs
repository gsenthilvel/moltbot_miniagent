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

        var lastUserMessage = sessionMessages
            .LastOrDefault(m => m.Role == "user")?.Content ?? "";

        var relevantSkills = RankSkills(skills, lastUserMessage);

        sb.AppendLine("\n\n## Available skills\n");
        foreach (var (skill, includeInstructions) in relevantSkills)
        {
            sb.AppendLine($"<skill name=\"{EscapeXml(skill.Name)}\" description=\"{EscapeXml(skill.Description)}\"/>");
            if (includeInstructions && !string.IsNullOrWhiteSpace(skill.Instructions))
            {
                sb.AppendLine();
                sb.AppendLine(skill.Instructions.Trim());
                sb.AppendLine();
            }
        }
        sb.AppendLine("\n## Available tools\n");
        foreach (var t in tools)
            sb.AppendLine($"- {t.Name}: {t.Description}");
        return sb.ToString();
    }

    private static List<(Skill skill, bool includeInstructions)> RankSkills(
        IReadOnlyList<Skill> skills, string userMessage)
    {
        var msg = userMessage.ToLowerInvariant();
        var result = new List<(Skill, bool)>();
        foreach (var s in skills)
        {
            var nameMatch = msg.Contains(s.Name.ToLowerInvariant());
            var descWords = s.Description.ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var descMatch = descWords.Count(w => w.Length > 3 && msg.Contains(w)) >= 2;

            var keywords = ExtractKeywords(s.Name);
            var keywordMatch = keywords.Any(k => msg.Contains(k));

            var isRelevant = nameMatch || descMatch || keywordMatch;
            result.Add((s, isRelevant));
        }
        return result;
    }

    private static string[] ExtractKeywords(string skillName)
    {
        var parts = skillName.ToLowerInvariant()
            .Split('-', StringSplitOptions.RemoveEmptyEntries);
        return parts.Where(p => p.Length > 3 && p != "mapbox" && p != "patterns").ToArray();
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
