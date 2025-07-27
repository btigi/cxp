using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using cxp.Models;

namespace cxp.Services;

public class UserDataService
{
    private static readonly string DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cxp");

    static UserDataService()
    {
        Directory.CreateDirectory(DataDirectory);
    }

    public static string? GetCurrentGitEmail()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ConfigurationService.Settings.Git.ExecutablePath,
                    Arguments = "config user.email",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            return process.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? GetProfileFilename(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(email));
        return Convert.ToHexString(hash).ToLower() + ".json";
    }

    public static UserData GetDefaultUserData(string? email = null)
    {
        return new UserData
        {
            Config = new UserConfig
            {
                UserEmail = email
            },
            User = new UserStats
            {
                Xp = 0,
                Level = 1
            },
            AchievementsUnlocked = [],
            Stats = new GameStats
            {
                TotalCommits = 0,
                TotalPushes = 0,
                LastCommitDate = "1970-01-01",
                LastPushDate = "1970-01-01",
                ConsecutiveCommitDays = 0,
                BranchesCreated = 0,
                MergesCompleted = 0,
                LogViews = 0,
                StashUses = 0,
                TagsCreated = 0,
                RevertsUsed = 0,
                CommitsByHour = new int[24],
                FilesAdded = 0,
                FilesDeleted = 0
            }
        };
    }

    public static UserData LoadUserData()
    {
        var email = GetCurrentGitEmail();
        if (string.IsNullOrEmpty(email))
            return GetDefaultUserData();

        var filename = GetProfileFilename(email);
        if (filename == null)
            return GetDefaultUserData();

        var profilePath = Path.Combine(DataDirectory, filename);

        if (!File.Exists(profilePath))
        {
            var userData = GetDefaultUserData(email);
            SaveUserData(userData);
            return userData;
        }

        try
        {
            var json = File.ReadAllText(profilePath);
            var diskData = JsonSerializer.Deserialize(json, JsonContext.Default.UserData);

            if (diskData == null)
            {
                var userData = GetDefaultUserData(email);
                SaveUserData(userData);
                return userData;
            }

            diskData.Config.UserEmail ??= email;
            return diskData;
        }
        catch (JsonException)
        {
            var userData = GetDefaultUserData(email);
            SaveUserData(userData);
            return userData;
        }
    }

    public static void SaveUserData(UserData data)
    {
        var email = data.Config.UserEmail;
        if (string.IsNullOrEmpty(email))
            return;

        var filename = GetProfileFilename(email);
        if (filename == null)
            return;

        var profilePath = Path.Combine(DataDirectory, filename);
        var options = new JsonSerializerOptions(JsonContext.Default.Options)
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(data, JsonContext.Default.UserData);

        File.WriteAllText(profilePath, json);
    }

    public static bool ClearUserData()
    {
        var email = GetCurrentGitEmail();
        if (string.IsNullOrEmpty(email))
            return false;

        var filename = GetProfileFilename(email);
        if (filename == null)
            return false;

        var profilePath = Path.Combine(DataDirectory, filename);

        if (File.Exists(profilePath))
        {
            try
            {
                File.Delete(profilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return true;
    }
}