using System.Text.Json.Serialization;

namespace cxp.Models;

public class LevelDefinition
{
    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("xp_required")]
    public int XpRequired { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}