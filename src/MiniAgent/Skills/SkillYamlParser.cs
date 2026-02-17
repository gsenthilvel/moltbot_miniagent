using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MiniAgent.Skills;

public static class SkillYamlParser
{
    private static readonly IDeserializer Yaml = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static (string name, string description, string instructions) Parse(string skillMdContent)
    {
        var instructions = skillMdContent;
        string name = "", description = "";

        if (skillMdContent.StartsWith("---", StringComparison.Ordinal))
        {
            var end = skillMdContent.IndexOf("---", 3, StringComparison.Ordinal);
            if (end >= 0)
            {
                var frontMatter = skillMdContent[3..end].Trim();
                instructions = skillMdContent[(end + 3)..].Trim();
                var fm = Yaml.Deserialize<Dictionary<string, object>>(frontMatter);
                if (fm != null)
                {
                    name = GetString(fm, "name");
                    description = GetString(fm, "description");
                }
            }
        }

        return (name, description, instructions);
    }

    private static string GetString(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var v) || v == null) return "";
        return v.ToString() ?? "";
    }
}
