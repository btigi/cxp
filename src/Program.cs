using System.CommandLine;
using cxp.Services;
using cxp.Models;
using Spectre.Console;
using System.Text.Json;

namespace cxp;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length > 0)
        {
            var firstArg = args[0].ToLower();
            if (firstArg == "profile" || firstArg == "help")
            {
                return await HandleInternalCommandAsync(args);
            }
        }

        return await RunGitWrapperAsync(args);
    }

    static async Task<int> HandleInternalCommandAsync(string[] args)
    {
        var rootCommand = new RootCommand("Gamify your Git experience")
        {
            Name = "cxp"
        };

        var profileCommand = new Command("profile", "Display user profile, stats, or clear progress");
        var statsOption = new Option<bool>("--stats", "Display detailed statistics and achievement descriptions") { IsRequired = false };
        var clearOption = new Option<bool>("--clear", "Clear all progress for the current user") { IsRequired = false };
        var detailsOption = new Option<bool>("--details", "Show all possible achievements") { IsRequired = false };
        profileCommand.AddOption(statsOption);
        profileCommand.AddOption(clearOption);
        profileCommand.AddOption(detailsOption);
        profileCommand.SetHandler(async (bool stats, bool clear, bool details) =>
        {
            await HandleProfileCommandAsync(stats, clear, details);
        }, statsOption, clearOption, detailsOption);

        var helpCommand = new Command("help", "Show the help message");
        helpCommand.SetHandler(() =>
        {
            ShowHelp();
        });

        rootCommand.AddCommand(profileCommand);
        rootCommand.AddCommand(helpCommand);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task<int> RunGitWrapperAsync(string[] gitArgs)
    {
        try
        {
            var result = await GitService.RunGitCommandAsync(gitArgs);

            if (result.ExitCode == 0)
            {
                var command = gitArgs.Length > 0 ? gitArgs[0] : "";
                if (command == "commit" || command == "push")
                {
                    AnsiConsole.MarkupLine("[dim]" + new string('-', 20) + "[/]");
                    await GamificationService.ProcessGamificationLogicAsync(gitArgs);
                }
            }

            return result.ExitCode;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    static Task HandleProfileCommandAsync(bool stats, bool clear, bool details)
    {
        var email = UserDataService.GetCurrentGitEmail();
        if (string.IsNullOrEmpty(email))
        {
            AnsiConsole.MarkupLine("[red]Error: Cannot find Git user email.[/]");
            AnsiConsole.MarkupLine("Please run [cyan]git config --global user.email 'your@email.com'[/] to set your identity.");
            return Task.CompletedTask;
        }

        if (clear)
        {
            if (AnsiConsole.Confirm($"[yellow]Are you sure you want to clear all progress for '{email}'?[/]"))
            {
                if (UserDataService.ClearUserData())
                {
                    AnsiConsole.MarkupLine($"[green]Profile for '{email}' has been successfully cleared![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to clear profile for '{email}'.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[cyan]Clear cancelled.[/]");
            }
            return Task.CompletedTask;
        }

        var userData = UserDataService.LoadUserData();

        if (stats)
        {
            DisplayDetailedStats(userData);
            return Task.CompletedTask;
        }

        if (details)
        {
            DisplayAllAchievements(userData);
            return Task.CompletedTask;
        }

        DisplayProfile(userData);
        return Task.CompletedTask;
    }

    static void DisplayProfile(UserData userData)
    {
        var user = userData.User;
        var level = user.Level;
        var xp = user.Xp;
        var levelName = LevelService.GetLevelName(level);
        var currentLevelXp = LevelService.GetXpForLevel(level);
        var nextLevelXp = LevelService.GetXpForNextLevel(level);
        var progressValue = xp - currentLevelXp;
        var progressTotal = nextLevelXp - currentLevelXp;

        if (progressTotal <= 0)
            progressTotal = 1;

        var profileText = $"Email: [cyan]{userData.Config.UserEmail}[/]\n" +
                          $"Level: {level} - {levelName}\n\n" +
                          $"XP Progress:";

        var progress = progressValue / (double)progressTotal;
        var progressText = $" {progressValue}/{progressTotal} ({progress:P1})";

        var panel = new Panel($"{profileText}\n{progressText}")
        {
            Header = new PanelHeader("cxp profile"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);

        if (userData.AchievementsUnlocked.Count > 0)
        {
            var achievementsList = string.Join("\n", userData.AchievementsUnlocked.Keys.Select(id => $"* {GetAchievementNameFromId(id)}"));

            var achievementsPanel = new Panel(achievementsList)
            {
                Header = new PanelHeader("Achievements Unlocked"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(achievementsPanel);
        }
    }

    static void DisplayDetailedStats(UserData userData)
    {
        var s = userData.Stats;
        
        var statsText = $"Total commits: {s.TotalCommits}\n" +
                        $"Total pushes: {s.TotalPushes}\n" +
                        $"Consecutive commit days: {s.ConsecutiveCommitDays}\n" +
                        $"Branches created: {s.BranchesCreated}\n" +
                        $"Merges completed: {s.MergesCompleted}\n" +
                        $"Log views: {s.LogViews}\n" +
                        $"Stash uses: {s.StashUses}\n" +
                        $"Tags created: {s.TagsCreated}\n" +
                        $"Reverts used: {s.RevertsUsed}\n" +
                        $"Files added: {s.FilesAdded}\n" +
                        $"Files deleted: {s.FilesDeleted}";

        var statsPanel = new Panel(statsText)
        {
            Header = new PanelHeader("Detailed Statistics"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(statsPanel);

        if (userData.AchievementsUnlocked.Count > 0)
        {
            var achievementsList = GetDetailedAchievementsList(userData.AchievementsUnlocked.Keys);

            var achievementsPanel = new Panel(achievementsList)
            {
                Header = new PanelHeader("Achievements Unlocked"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(achievementsPanel);
        }
    }

    static void DisplayAllAchievements(UserData userData)
    {
        var allAchievements = LoadAllAchievements();
        var unlockedIds = userData.AchievementsUnlocked.Keys.ToHashSet();

        var achievementsList = new List<string>();

        foreach (var category in allAchievements)
        {
            achievementsList.Add($"{category.Key.ToUpper()}");
            
            foreach (var achievement in category.Value)
            {
                var id = achievement.Key;
                var info = achievement.Value;
                var isUnlocked = unlockedIds.Contains(id);
                
                if (isUnlocked)
                {
                    achievementsList.Add($"(x) {info.Name}");
                    achievementsList.Add($"   {info.Description}");
                    achievementsList.Add($"   XP Reward: {info.XpReward}");
                }
                else
                {
                    achievementsList.Add($"( ) {info.Name}");
                    achievementsList.Add($"   {info.Description}");
                    achievementsList.Add($"   XP Reward: {info.XpReward}");
                }
                achievementsList.Add("");
            }
        }

        if (achievementsList.Count > 0 && string.IsNullOrEmpty(achievementsList.Last()))
        {
            achievementsList.RemoveAt(achievementsList.Count - 1);
        }

        var panel = new Panel(string.Join("\n", achievementsList))
        {
            Header = new PanelHeader("All Achievements"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }

    static Dictionary<string, Dictionary<string, Achievement>> LoadAllAchievements()
    {
        try
        {
            var achievementsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CxpConfig", "achievements.json");
            if (File.Exists(achievementsPath))
            {
                var json = File.ReadAllText(achievementsPath);
                var categories = JsonSerializer.Deserialize(json, JsonContext.Default.DictionaryStringDictionaryStringAchievement);
                return categories ?? new Dictionary<string, Dictionary<string, Achievement>>();
            }
        }
        catch
        {
            // Fall through to default
        }

        return new Dictionary<string, Dictionary<string, Achievement>>();
    }

    static string GetDetailedAchievementsList(IEnumerable<string> achievementIds)
    {
        var achievements = new List<string>();
        
        foreach (var achievementId in achievementIds)
        {
            var name = GetAchievementNameFromId(achievementId);
            var description = GetAchievementDescriptionFromId(achievementId);
            achievements.Add($"[bold cyan]* {name}[/]\n  [italic]{description}[/]");
        }
        
        return string.Join("\n\n", achievements);
    }

    static string GetAchievementDescriptionFromId(string achievementId)
    {
        try
        {
            var achievementsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CxpConfig", "achievements.json");
            if (File.Exists(achievementsPath))
            {
                var json = File.ReadAllText(achievementsPath);
                var categories = JsonSerializer.Deserialize(json, JsonContext.Default.DictionaryStringDictionaryStringAchievement);
                
                if (categories != null)
                {
                    foreach (var category in categories.Values)
                    {
                        if (category.TryGetValue(achievementId, out var achievement))
                        {
                            return achievement.Description;
                        }
                    }
                }
            }
        }
        catch
        {
            // Fall through to default
        }
        
        return "Achievement unlocked!";
    }

    static void ShowHelp()
    {
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn(new TableColumn("Command").Width(12))
            .AddColumn("Description");

        table.AddRow("[cyan]profile[/]", "Display user profile, stats, or clear progress.");
        table.AddRow("[cyan]help[/]", "Show this help message and exit.");

        var panel = new Panel(
            Align.Left(
                new Rows(
                    new Text("Usage: cxp [GIT_COMMAND] or cxp [INTERNAL_COMMAND] [OPTIONS]..."),
                    new Text("Earn XP and level up with every git command!\n"),
                    new Text("Examples:"),
                    new Text("  cxp commit -m \"Add feature\"  # Same as: git commit -m \"Add feature\""),
                    new Text("  cxp push origin main          # Same as: git push origin main"),
                    new Text("  cxp profile --stats           # Internal command\n"),
                    new Text("Internal Commands:"),
                    table
                )
            )
        )
        {
            Header = new PanelHeader("cxp Help"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }

    static string GetAchievementNameFromId(string achievementId)
    {
        try
        {
            var achievementsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CxpConfig", "achievements.json");
            if (File.Exists(achievementsPath))
            {
                var json = File.ReadAllText(achievementsPath);
                var categories = JsonSerializer.Deserialize(json, JsonContext.Default.DictionaryStringDictionaryStringAchievement);

                if (categories != null)
                {
                    foreach (var category in categories.Values)
                    {
                        if (category.TryGetValue(achievementId, out var achievement))
                        {
                            return achievement.Name;
                        }
                    }
                }
            }
        }
        catch
        {
            // Fall through to default
        }

        return "Unknown Achievement";
    }
}