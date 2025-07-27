using System.Text.Json.Serialization;

namespace cxp.Models;

public class UserConfig
{
    [JsonPropertyName("user_email")]
    public string? UserEmail { get; set; }
}