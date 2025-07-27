using System.Text.Json;
using cxp.Models;

namespace cxp.Services;

public static class XpConfigService
{
    private static XpConfig? _config;
    private static readonly object _lock = new object();

    public static XpConfig Config
    {
        get
        {
            if (_config == null)
            {
                lock (_lock)
                {
                    if (_config == null)
                    {
                        LoadConfig();
                    }
                }
            }
            return _config!;
        }
    }

    private static void LoadConfig()
    {
        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CxpConfig", "xp.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                _config = JsonSerializer.Deserialize(json, JsonContext.Default.XpConfig);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load xp.json: {ex.Message}");
        }

        // Fallback to default config if loading failed
        _config ??= new XpConfig
        {
            Commit = new Dictionary<string, int> { ["1-100"] = 10 },
            Push = new Dictionary<string, int> { ["1-100"] = 15 },
            Branch = new Dictionary<string, int> { ["1-100"] = 25 },
            Merge = new Dictionary<string, int> { ["1-100"] = 30 },
            Log = new Dictionary<string, int> { ["1-100"] = 5 },
            Stash = new Dictionary<string, int> { ["1-100"] = 25 },
            Tag = new Dictionary<string, int> { ["1-100"] = 20 },
            Revert = new Dictionary<string, int> { ["1-100"] = 35 }
        };
    }

    public static int GetXpForAction(string action, int userLevel)
    {
        var actionConfig = action.ToLower() switch
        {
            "commit" => Config.Commit,
            "push" => Config.Push,
            "branch" => Config.Branch,
            "merge" => Config.Merge,
            "log" => Config.Log,
            "stash" => Config.Stash,
            "tag" => Config.Tag,
            "revert" => Config.Revert,
            _ => []
        };

        foreach (var range in actionConfig)
        {
            if (IsLevelInRange(userLevel, range.Key))
            {
                return range.Value;
            }
        }

        return 0; // No XP if no range matches
    }

    private static bool IsLevelInRange(int level, string range)
    {
        try
        {
            if (range.Contains('-'))
            {
                var parts = range.Split('-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out var min) &&
                    int.TryParse(parts[1], out var max))
                {
                    return level >= min && level <= max;
                }
            }
            else if (int.TryParse(range, out var exactLevel))
            {
                return level == exactLevel;
            }
        }
        catch
        {
            // Fall through to false
        }

        return false;
    }
}