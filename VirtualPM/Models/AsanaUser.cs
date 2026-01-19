using System.Text.Json.Serialization;

namespace VirtualPM.Models;

public class AsanaUser
{
    [JsonPropertyName("gid")]
    public string Gid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
