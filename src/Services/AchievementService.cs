using System.Text.Json;
using cxp.Models;
using Spectre.Console;

namespace cxp.Services;

public class AchievementService
{
    private readonly Dictionary<string, Achievement> _achievementConfig = [];

    public AchievementService()
    {
        LoadAchievementConfig();
    }

    private void LoadAchievementConfig()
    {
        try
        {
            var achievementsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CxpConfig", "achievements.json");
            if (!File.Exists(achievementsPath))
                return;

                            var json = File.ReadAllText(achievementsPath);
                var categories = JsonSerializer.Deserialize(json, JsonContext.Default.DictionaryStringDictionaryStringAchievement);

            if (categories != null)
            {
                foreach (var category in categories.Values)
                {
                    foreach (var achievement in category)
                    {
                        _achievementConfig[achievement.Key] = achievement.Value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load achievements: {ex.Message}");
        }
    }

    public async Task<int> CheckAllAchievementsAsync(UserData userData, GameContext context)
    {
        var xpFromAchievements = 0;

        var checkers = GetAchievementCheckers();

        foreach (var checker in checkers)
        {
            var achievementId = checker.Key;

            if (userData.AchievementsUnlocked.ContainsKey(achievementId))
                continue;

            var result = await checker.Value(userData, context);
            if (result?.Unlocked == true)
            {
                userData.AchievementsUnlocked[achievementId] = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

                if (_achievementConfig.TryGetValue(achievementId, out var achievement))
                {
                    xpFromAchievements += achievement.XpReward;
                    DisplayAchievementUnlocked(achievement);
                }
            }
        }

        return xpFromAchievements;
    }

    private Dictionary<string, Func<UserData, GameContext, Task<AchievementResult?>>> GetAchievementCheckers()
    {
        var checkers = new Dictionary<string, Func<UserData, GameContext, Task<AchievementResult?>>>();

        foreach (var achievementId in _achievementConfig.Keys)
        {
            var parts = achievementId.Split('_');
            if (parts.Length == 2 && int.TryParse(parts[1], out var targetValue))
            {
                var statType = parts[0];
                if (statType == "hour")
                {
                    var hour = targetValue;
                    if (hour >= 0 && hour <= 23)
                    {
                        checkers[achievementId] = (u, c) => Task.FromResult(CheckHourlyStat(u, hour, achievementId));
                        continue;
                    }
                }
                else
                {
                    var statKey = GetStatKeyFromType(statType);
                    if (statKey != null)
                    {
                        checkers[achievementId] = (u, c) => Task.FromResult(CheckStats(u, statKey, targetValue, achievementId));
                        continue;
                    }
                }
            }
        }

        return checkers;
    }

    private string? GetStatKeyFromType(string statType)
    {
        return statType switch
        {
            "commit" => "total_commits",
            "push" => "total_pushes",
            "merge" => "merges_completed",
            "branch" => "branches_created",
            "log" => "log_views",
            "stash" => "stash_uses",
            "tag" => "tags_created",
            "revert" => "reverts_used",
            "combo" => "consecutive_commit_days",
            "hour" => "commits_by_hour", // Special case, handled differently
            "builder" => "files_added",
            "destroyer" => "files_deleted",
            _ => null
        };
    }

    private AchievementResult? CheckStats(UserData userData, string statKey, int targetValue, string achievementId)
    {
        var statValue = statKey switch
        {
            "total_commits" => userData.Stats.TotalCommits,
            "total_pushes" => userData.Stats.TotalPushes,
            "consecutive_commit_days" => userData.Stats.ConsecutiveCommitDays,
            "branches_created" => userData.Stats.BranchesCreated,
            "merges_completed" => userData.Stats.MergesCompleted,
            "log_views" => userData.Stats.LogViews,
            "stash_uses" => userData.Stats.StashUses,
            "tags_created" => userData.Stats.TagsCreated,
            "reverts_used" => userData.Stats.RevertsUsed,
            "files_added" => userData.Stats.FilesAdded,
            "files_deleted" => userData.Stats.FilesDeleted,
            _ => 0
        };

        if (statValue >= targetValue)
        {
            return new AchievementResult { Id = achievementId, Unlocked = true };
        }

        return null;
    }

    private AchievementResult? CheckHourlyStat(UserData userData, int hour, string achievementId)
    {
        if (hour >= 0 && hour < 24 && userData.Stats.CommitsByHour[hour] >= 1)
        {
            return new AchievementResult { Id = achievementId, Unlocked = true };
        }
        return null;
    }

    private void DisplayAchievementUnlocked(Achievement achievement)
    {
        var name = achievement.Name;
        var description = achievement.Description;
        var panelTitle = "Achievement Unlocked!";

        var panel = new Panel($"[cyan]{name}[/]\n[italic]{description}[/]\n\nGained +{achievement.XpReward} XP!")
        {
            Header = new PanelHeader(panelTitle),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }
}