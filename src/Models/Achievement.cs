using System.Text.Json.Serialization;

namespace cxp.Models;

public class Achievement
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("xp_reward")]
    public int XpReward { get; set; } = 0;
}