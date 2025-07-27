using System.Text.Json.Serialization;

namespace cxp.Models;

public class UserStats
{
    [JsonPropertyName("xp")]
    public int Xp { get; set; } = 0;

    [JsonPropertyName("level")]
    public int Level { get; set; } = 1;
}