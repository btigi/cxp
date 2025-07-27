using System.Text.Json.Serialization;

namespace cxp.Models;

[JsonSerializable(typeof(UserData))]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(Achievement))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, Achievement>>))]
[JsonSerializable(typeof(List<LevelDefinition>))]
[JsonSerializable(typeof(XpConfig))]
public partial class JsonContext : JsonSerializerContext
{
}