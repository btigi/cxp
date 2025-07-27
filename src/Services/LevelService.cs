using cxp.Models;
using System.Text.Json;

namespace cxp.Services;

public static class LevelService
{
    private static List<LevelDefinition> _levels = [];
    private static bool _loaded = false;

    static LevelService()
    {
        LoadLevels();
    }

    private static void LoadLevels()
    {
        try
        {
            var levelsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CxpConfig", "levels.json");
            if (File.Exists(levelsPath))
            {
                var json = File.ReadAllText(levelsPath);
                var levels = JsonSerializer.Deserialize(json, JsonContext.Default.ListLevelDefinition);
                if (levels != null)
                {
                    _levels = levels.OrderBy(l => l.Level).ToList();
                    _loaded = true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load levels: {ex.Message}");
        }

        if (!_loaded || _levels.Count == 0)
        {
            _levels =
            [
                new() { Level = 1, XpRequired = 0, Name = "Git Developer" }
            ];
        }
    }

    public static string GetLevelName(int level)
    {
        if (level < 1)
            level = 1;

        var levelDef = _levels.Where(l => l.Level <= level).LastOrDefault();
        if (levelDef != null)
            return levelDef.Name;

        return _levels.LastOrDefault()?.Name ?? "Git Master";
    }

    public static int GetLevelFromXp(int xp)
    {
        if (xp < 0)
            xp = 0;

        var applicableLevel = _levels.Where(l => l.XpRequired <= xp).LastOrDefault();
        return applicableLevel?.Level ?? 1;
    }

    public static int GetXpForLevel(int level)
    {
        var levelDef = _levels.FirstOrDefault(l => l.Level == level);
        if (levelDef != null)
            return levelDef.XpRequired;

        var lastDefined = _levels.LastOrDefault();
        if (lastDefined != null && level > lastDefined.Level)
        {
            var levelsAbove = level - lastDefined.Level;
            return lastDefined.XpRequired + levelsAbove * 5000;
        }

        return 0;
    }

    public static int GetXpForNextLevel(int currentLevel)
    {
        return GetXpForLevel(currentLevel + 1);
    }
}