using System.Text.Json.Serialization;

namespace cxp.Models;

public class AppSettings
{
    [JsonPropertyName("git")]
    public GitSettings Git { get; set; } = new();
}

public class GitSettings
{
    [JsonPropertyName("executablePath")]
    public string ExecutablePath { get; set; } = "git";
}