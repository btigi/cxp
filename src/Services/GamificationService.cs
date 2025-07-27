using cxp.Models;
using Spectre.Console;

namespace cxp.Services;

public class GamificationService
{
    public static async Task ProcessGamificationLogicAsync(string[] gitArgs)
    {
        var userData = UserDataService.LoadUserData();
        
        if (string.IsNullOrEmpty(userData.Config.UserEmail))
            return;

        var command = gitArgs.Length > 0 ? gitArgs[0] : "";
        var today = DateOnly.FromDateTime(DateTime.Today);
        var xpToAdd = 0;
        var context = new GameContext { Command = command };

        if (command == "commit")
        {
            xpToAdd += await ProcessCommitAsync(userData, today, context);
        }
        else if (command == "push")
        {
            xpToAdd += ProcessPush(userData, today);
        }
        else if (command == "branch" || (command == "checkout" && gitArgs.Length > 1 && gitArgs[1] == "-b"))
        {
            xpToAdd += ProcessBranch(userData);
        }
        else if (command == "merge")
        {
            xpToAdd += ProcessMerge(userData);
        }
        else if (command == "log")
        {
            xpToAdd += ProcessLog(userData);
        }
        else if (command == "stash")
        {
            xpToAdd += ProcessStash(userData);
        }
        else if (command == "tag")
        {
            xpToAdd += ProcessTag(userData);
        }
        else if (command == "revert")
        {
            xpToAdd += ProcessRevert(userData);
        }

        if (xpToAdd <= 0)
            return;

        var achievementService = new AchievementService();
        var xpFromAchievements = await achievementService.CheckAllAchievementsAsync(userData, context);
        xpToAdd += xpFromAchievements;

        var currentLevel = userData.User.Level;
        var currentXp = userData.User.Xp;
        var newXp = currentXp + xpToAdd;
        var newLevel = LevelService.GetLevelFromXp(newXp);

        userData.User.Xp = newXp;
        userData.User.Level = newLevel;

        DisplayXpGain(xpToAdd, newLevel, newXp);

        if (newLevel > currentLevel)
        {
            DisplayLevelUp(newLevel);
        }

        UserDataService.SaveUserData(userData);
    }

    private static async Task<int> ProcessCommitAsync(UserData userData, DateOnly today, GameContext context)
    {
        userData.Stats.TotalCommits++;
        
        var currentHour = DateTime.Now.Hour;
        userData.Stats.CommitsByHour[currentHour]++;
        
        var commitDiffStats = await GitService.GetCommitDiffStatsAsync();
        if (commitDiffStats != null)
        {
            userData.Stats.FilesAdded += commitDiffStats.FilesAdded;
        }
        
        var lastCommitDate = userData.Stats.LastCommitDate != "1970-01-01" ? DateOnly.Parse(userData.Stats.LastCommitDate) : DateOnly.MinValue;

        if (lastCommitDate != DateOnly.MinValue)
        {
            var daysDiff = today.DayNumber - lastCommitDate.DayNumber;
            if (daysDiff == 1)
            {
                userData.Stats.ConsecutiveCommitDays++;
            }
            else if (daysDiff > 1)
            {
                userData.Stats.ConsecutiveCommitDays = 1;
            }
        }
        else
        {
            userData.Stats.ConsecutiveCommitDays = 1;
        }

        if (userData.Stats.LastCommitDate != today.ToString("yyyy-MM-dd"))
        {
            userData.Stats.LastCommitDate = today.ToString("yyyy-MM-dd");
        }

        var baseXp = XpConfigService.GetXpForAction("commit", userData.User.Level);
        var xp = baseXp;
        xp += Math.Min(userData.Stats.ConsecutiveCommitDays, 15);

        var diffStats = await GitService.GetCommitDiffStatsAsync();
        if (diffStats != null)
        {
            xp += Math.Min(diffStats.TotalChanges / 20, 20);
            context.Deletions = diffStats.Deletions;
        }

        var commitMessage = await GitService.GetLastCommitMessageAsync();
        if (commitMessage != null)
        {
            context.CommitMessage = commitMessage;
        }

        return xp;
    }

    private static int ProcessPush(UserData userData, DateOnly today)
    {
        userData.Stats.TotalPushes++;
        
        var lastPushDate = userData.Stats.LastPushDate != "1970-01-01" 
            ? DateOnly.Parse(userData.Stats.LastPushDate) 
            : DateOnly.MinValue;

        var xp = XpConfigService.GetXpForAction("push", userData.User.Level);
        
        if (today != lastPushDate)
        {
            xp += 50;
            userData.Stats.LastPushDate = today.ToString("yyyy-MM-dd");
        }

        return xp;
    }

    private static int ProcessBranch(UserData userData)
    {
        userData.Stats.BranchesCreated++;
        return XpConfigService.GetXpForAction("branch", userData.User.Level);
    }

    private static int ProcessMerge(UserData userData)
    {
        userData.Stats.MergesCompleted++;
        return XpConfigService.GetXpForAction("merge", userData.User.Level);
    }

    private static int ProcessLog(UserData userData)
    {
        userData.Stats.LogViews++;
        return XpConfigService.GetXpForAction("log", userData.User.Level);
    }

    private static int ProcessStash(UserData userData)
    {
        userData.Stats.StashUses++;
        return XpConfigService.GetXpForAction("stash", userData.User.Level);
    }

    private static int ProcessTag(UserData userData)
    {
        userData.Stats.TagsCreated++;
        return XpConfigService.GetXpForAction("tag", userData.User.Level);
    }

    private static int ProcessRevert(UserData userData)
    {
        userData.Stats.RevertsUsed++;
        return XpConfigService.GetXpForAction("revert", userData.User.Level);
    }

    private static void DisplayXpGain(int xpGained, int level, int totalXp)
    {
        var nextLevelXp = LevelService.GetXpForNextLevel(level);

        var message = $"You gained +{xpGained} XP! Current level {level} ({totalXp}/{nextLevelXp}).";

        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }

    private static void DisplayLevelUp(int newLevel)
    {
        var levelName = LevelService.GetLevelName(newLevel);
        
        var levelUpMessage = $"LEVEL UP! You have reached Level {newLevel}: {levelName}!";

        AnsiConsole.MarkupLine($"[magenta]{levelUpMessage}[/]");
    }
}