namespace MiniAgent.Skills;

public class SkillLoader
{
    private readonly string _workspaceRoot;
    private readonly string _userSkillsDir;
    private readonly IEnumerable<string> _extraDirs;

    public SkillLoader(string workspaceRoot, IEnumerable<string>? extraDirs = null)
    {
        _workspaceRoot = Path.GetFullPath(workspaceRoot);
        _userSkillsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".miniagent", "skills");
        _extraDirs = extraDirs ?? Array.Empty<string>();
    }

    public IReadOnlyList<Skill> Load()
    {
        var byName = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);

        void AddFromDir(string dir, bool overwrite)
        {
            if (!Directory.Exists(dir)) return;
            foreach (var skillDir in Directory.GetDirectories(dir))
            {
                var skillPath = Path.Combine(skillDir, "SKILL.md");
                if (!File.Exists(skillPath)) continue;
                var content = File.ReadAllText(skillPath);
                var (name, description, instructions) = SkillYamlParser.Parse(content);
                if (string.IsNullOrWhiteSpace(name)) name = Path.GetFileName(skillDir);
                if (overwrite || !byName.ContainsKey(name))
                    byName[name] = new Skill
                    {
                        Name = name,
                        Description = description,
                        Instructions = instructions,
                        Location = skillDir
                    };
            }
        }

        // Lowest precedence first: extra dirs, then user, then workspace
        foreach (var d in _extraDirs)
            AddFromDir(Path.GetFullPath(d), overwrite: false);
        AddFromDir(_userSkillsDir, overwrite: false);
        AddFromDir(Path.Combine(_workspaceRoot, "skills"), overwrite: false);
        // Workspace overwrites (highest precedence) - we already added workspace last so it wins

        return byName.Values.ToList();
    }
}
