using System.Text.Json.Serialization;

namespace cxp.Models;

public class UserData
{
    [JsonPropertyName("config")]
    public UserConfig Config { get; set; } = new();

    [JsonPropertyName("user")]
    public UserStats User { get; set; } = new();

    [JsonPropertyName("achievements_unlocked")]
    public Dictionary<string, string> AchievementsUnlocked { get; set; } = [];

    [JsonPropertyName("stats")]
    public GameStats Stats { get; set; } = new();
}