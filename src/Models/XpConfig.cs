using System.Text.Json.Serialization;

namespace cxp.Models;

public class XpConfig
{
    [JsonPropertyName("commit")]
    public Dictionary<string, int> Commit { get; set; } = [];

    [JsonPropertyName("push")]
    public Dictionary<string, int> Push { get; set; } = [];

    [JsonPropertyName("branch")]
    public Dictionary<string, int> Branch { get; set; } = [];

    [JsonPropertyName("merge")]
    public Dictionary<string, int> Merge { get; set; } = [];

    [JsonPropertyName("log")]
    public Dictionary<string, int> Log { get; set; } = [];

    [JsonPropertyName("stash")]
    public Dictionary<string, int> Stash { get; set; } = [];

    [JsonPropertyName("tag")]
    public Dictionary<string, int> Tag { get; set; } = [];

    [JsonPropertyName("revert")]
    public Dictionary<string, int> Revert { get; set; } = [];
}